CREATE MIGRATION m1wvcf5q6yezhhrvo6osbmy7haxeebjkieb4mldwemeste5njb22ja
    ONTO m1xmoez2o6q7vndjmc6wgvt37aboqzz3olnkeexw37yli3u3zpfpsa
{
  ALTER TYPE default::Contact {
      ALTER PROPERTY date_of_birth {
          SET TYPE cal::local_date USING (<cal::local_date>'2018-05-07');
      };
  };
};
