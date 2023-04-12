set search_path = 'era';

--create extensions for calculating earth distance
CREATE EXTENSION IF NOT EXISTS cube WITH SCHEMA public;
CREATE EXTENSION IF NOT EXISTS earthdistance WITH SCHEMA public;

--create customer table
CREATE TABLE IF NOT EXISTS public.customer (
   id serial PRIMARY KEY,
   has_reservation boolean default false not null
);

--create assistant table
CREATE TABLE IF NOT EXISTS public.assistant (
   id serial PRIMARY KEY,
   location point,
   is_reserved bool default false not null
);


--create reservation table
CREATE TABLE IF NOT EXISTS public.reservation (
   id serial PRIMARY KEY,
   customer_id integer,
   assistant_id integer,
   is_reserved bool default false not null,
   updated_on timestamptz,
   created_on timestamptz
);

--add customers and assistants
insert into public.customer values (1);
insert into public.customer values (2);
insert into public.customer values (3);
insert into public.assistant values (1, point(-85.731925,38.236837));
insert into public.assistant values (2, point(-85.741025,38.227837));
insert into public.assistant values (3, point(-85.751925,38.228837));
insert into public.assistant values (4, point(-85.761025,38.229837));
insert into public.assistant values (5, point(-85.771925,38.220837));
insert into public.assistant values (6, point(-85.781025,38.221837));
insert into public.assistant values (7, point(-85.791925,38.222837));
insert into public.assistant values (8, point(-85.799025,38.233837));

-- update location of a assistant
update public.assistant set location = point(38.226837, -85.781099) where id = 6;

-- reserve a assistant to a customer
insert into public.reservation (customer_id, assistant_id, is_reserved, created_on, updated_on) values(1, 2, true, now(), null);
update public.assistant set is_reserved=true where id = 2;

-- release a assistant from a customer
update public.reservation set is_reserved=false, updated_on = now() where customer_id = 1 and assistant_id = 2 and is_reserved = true;
update public.assistant set is_reserved=false where id = 2 returning is_reserved;
