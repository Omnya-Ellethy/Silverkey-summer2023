CREATE MIGRATION m1xmoez2o6q7vndjmc6wgvt37aboqzz3olnkeexw37yli3u3zpfpsa
    ONTO m16w6r2lzr45znpdaadaydovh26mzf4ahop7achopb4rxx664u43oa
{
  ALTER TYPE default::Contact {
      ALTER PROPERTY date_of_birth {
          SET TYPE std::datetime USING (<std::datetime>'2018-05-07T15:01:22.306916+00');
      };
  };
};
