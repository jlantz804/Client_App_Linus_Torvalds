ALTER TABLE public.lesson_plans
    ADD COLUMN IF NOT EXISTS locked boolean NOT NULL DEFAULT false,
    ADD COLUMN IF NOT EXISTS created_by uuid;

ALTER TABLE public.lesson_plans
    ALTER COLUMN created_by DROP DEFAULT,
    ALTER COLUMN created_by DROP NOT NULL;

DO $$
DECLARE
    created_by_type text;
BEGIN
    SELECT data_type
    INTO created_by_type
    FROM information_schema.columns
    WHERE table_schema = 'public'
      AND table_name = 'lesson_plans'
      AND column_name = 'created_by';

    IF created_by_type <> 'uuid' THEN
        ALTER TABLE public.lesson_plans
            ALTER COLUMN created_by TYPE uuid
            USING CASE
                WHEN created_by::text ~* '^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$'
                    THEN created_by::text::uuid
                ELSE NULL
            END;
    END IF;
END
$$;

CREATE TABLE IF NOT EXISTS public.users (
    uid uuid PRIMARY KEY REFERENCES auth.users(id) ON DELETE CASCADE,
    name text NOT NULL,
    is_admin boolean NOT NULL DEFAULT false
);

ALTER TABLE public.users
    ADD COLUMN IF NOT EXISTS is_admin boolean NOT NULL DEFAULT false;

CREATE OR REPLACE FUNCTION public.is_admin()
RETURNS boolean
LANGUAGE sql
STABLE
SECURITY DEFINER
SET search_path = public
AS $$
    SELECT EXISTS (
        SELECT 1
        FROM public.users
        WHERE uid = auth.uid()
          AND is_admin = true
    );
$$;

CREATE OR REPLACE FUNCTION public.handle_new_auth_user()
RETURNS trigger
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = public
AS $$
BEGIN
    INSERT INTO public.users (uid, name)
    VALUES (
        NEW.id,
        COALESCE(NULLIF(NEW.raw_user_meta_data ->> 'name', ''), NULLIF(NEW.email, ''), 'Teacher')
    )
    ON CONFLICT (uid) DO UPDATE
    SET name = EXCLUDED.name;
    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS on_auth_user_created_profile ON auth.users;
CREATE TRIGGER on_auth_user_created_profile
    AFTER INSERT ON auth.users
    FOR EACH ROW EXECUTE FUNCTION public.handle_new_auth_user();

CREATE OR REPLACE FUNCTION public.protect_lesson_plan_owner_and_lock()
RETURNS trigger
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = public
AS $$
BEGIN
    IF NEW.created_by IS DISTINCT FROM OLD.created_by THEN
        RAISE EXCEPTION 'The lesson-plan creator cannot be changed.';
    END IF;

    IF (OLD.locked OR NEW.locked IS DISTINCT FROM OLD.locked)
       AND OLD.created_by IS DISTINCT FROM auth.uid()
       AND NOT public.is_admin() THEN
        RAISE EXCEPTION 'Only the lesson-plan creator or an admin can edit a locked plan or change its lock status.';
    END IF;

    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS protect_lesson_plan_owner_and_lock ON public.lesson_plans;
CREATE TRIGGER protect_lesson_plan_owner_and_lock
    BEFORE UPDATE ON public.lesson_plans
    FOR EACH ROW EXECUTE FUNCTION public.protect_lesson_plan_owner_and_lock();

DO $$
DECLARE
    existing_policy record;
BEGIN
    FOR existing_policy IN
        SELECT policyname, tablename
        FROM pg_policies
        WHERE schemaname = 'public'
          AND tablename IN ('lesson_plans', 'users')
    LOOP
        EXECUTE format('DROP POLICY %I ON public.%I', existing_policy.policyname, existing_policy.tablename);
    END LOOP;
END
$$;

ALTER TABLE public.users ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.lesson_plans ENABLE ROW LEVEL SECURITY;

REVOKE ALL ON public.users FROM anon, authenticated;
GRANT SELECT ON public.users TO authenticated;
GRANT SELECT, INSERT, UPDATE, DELETE ON public.lesson_plans TO authenticated;

CREATE POLICY users_select_self_or_admin
    ON public.users FOR SELECT TO authenticated
    USING (
        auth.jwt() ->> 'is_anonymous' IS DISTINCT FROM 'true'
        AND (uid = auth.uid() OR public.is_admin())
    );

CREATE POLICY lesson_plans_select_authenticated
    ON public.lesson_plans FOR SELECT TO authenticated
    USING (auth.jwt() ->> 'is_anonymous' IS DISTINCT FROM 'true');

CREATE POLICY lesson_plans_insert_creator
    ON public.lesson_plans FOR INSERT TO authenticated
    WITH CHECK (
        auth.jwt() ->> 'is_anonymous' IS DISTINCT FROM 'true'
        AND created_by = auth.uid()
    );

CREATE POLICY lesson_plans_update_creator_admin_or_unlocked
    ON public.lesson_plans FOR UPDATE TO authenticated
    USING (
        auth.jwt() ->> 'is_anonymous' IS DISTINCT FROM 'true'
        AND (NOT locked OR created_by = auth.uid() OR public.is_admin())
    )
    WITH CHECK (
        auth.jwt() ->> 'is_anonymous' IS DISTINCT FROM 'true'
        AND (NOT locked OR created_by = auth.uid() OR public.is_admin())
    );

CREATE POLICY lesson_plans_delete_creator_admin_or_unlocked
    ON public.lesson_plans FOR DELETE TO authenticated
    USING (
        auth.jwt() ->> 'is_anonymous' IS DISTINCT FROM 'true'
        AND (NOT locked OR created_by = auth.uid() OR public.is_admin())
    );
