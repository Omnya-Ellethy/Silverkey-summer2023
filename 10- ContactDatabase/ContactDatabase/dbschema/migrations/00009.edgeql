CREATE MIGRATION m1mhia2aqsptua6mi4udl6dxladkf7qjyty2hcokv6glv7d4xazarq
    ONTO m13754fze5qyiuooxcqlqwdee4fjmah5ecnz34xk4kjmoaj6owyflq
{
  ALTER TYPE default::Contact {
      ALTER PROPERTY date_of_birth {
          SET TYPE std::str USING (<std::str>.date_of_birth);
      };
  };
};
