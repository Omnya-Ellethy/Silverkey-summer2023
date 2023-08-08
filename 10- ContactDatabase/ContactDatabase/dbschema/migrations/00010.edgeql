CREATE MIGRATION m16w6r2lzr45znpdaadaydovh26mzf4ahop7achopb4rxx664u43oa
    ONTO m1mhia2aqsptua6mi4udl6dxladkf7qjyty2hcokv6glv7d4xazarq
{
  ALTER TYPE default::Contact {
      ALTER PROPERTY date_of_birth {
          SET TYPE cal::local_datetime USING (<cal::local_datetime>'2023-07-04T00:00:00');
      };
  };
};
