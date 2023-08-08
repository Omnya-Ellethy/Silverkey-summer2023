CREATE MIGRATION m16mz75ghwaqbkb6f6lmp2paomlhjyv3r6symv6tkn22fw4u5wus7q
    ONTO m1wvcf5q6yezhhrvo6osbmy7haxeebjkieb4mldwemeste5njb22ja
{
  ALTER TYPE default::Contact {
      ALTER PROPERTY date_of_birth {
          SET TYPE std::datetime USING (<std::datetime>'2023-05-07T15:01:22+00');
      };
  };
};
