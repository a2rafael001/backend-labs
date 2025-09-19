using FluentMigrator;

namespace Migrations.Scripts;

[Migration(202509130001)]
public class InitOrders : Migration
{
    public override void Up()
    {
        // таблица заказов
        Execute.Sql(@"
            create table if not exists public.orders (
                id bigserial primary key,
                customer_id bigint not null,
                total_price_cents bigint not null,
                total_price_currency text not null,
                created_at timestamptz not null default now()
            );
            create index if not exists idx_orders_customer_id on public.orders (customer_id);
        ");

        // позиции заказа (FK можно не добавлять в учебной работе для скорости)
        Execute.Sql(@"
            create table if not exists public.order_items (
                id bigserial primary key,
                order_id bigint not null,
                product_id bigint not null,
                product_name text not null,
                price_cents bigint not null,
                price_currency text not null,
                quantity integer not null
            );
            create index if not exists idx_order_items_order_id on public.order_items (order_id);
        ");

        // композитные типы для будущих bulk-вставок
        Execute.Sql(@"
            do $$
            begin
                if not exists (select 1 from pg_type where typname = 'v1_order') then
                    create type public.v1_order as (
                        customer_id bigint,
                        total_price_cents bigint,
                        total_price_currency text
                    );
                end if;

                if not exists (select 1 from pg_type where typname = 'v1_order_item') then
                    create type public.v1_order_item as (
                        order_id bigint,
                        product_id bigint,
                        product_name text,
                        price_cents bigint,
                        price_currency text,
                        quantity integer
                    );
                end if;
            end
            $$;
        ");
    }

    public override void Down()
    {
        Execute.Sql(@"drop table if exists public.order_items;");
        Execute.Sql(@"drop table if exists public.orders;");
        Execute.Sql(@"drop type if exists public.v1_order_item;");
        Execute.Sql(@"drop type if exists public.v1_order;");
    }
}
