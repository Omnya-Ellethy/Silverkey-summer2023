CREATE MIGRATION m13754fze5qyiuooxcqlqwdee4fjmah5ecnz34xk4kjmoaj6owyflq
    ONTO m1uwy6xdr34dflpvbe6ohbcfeoc2fkk7gfk6na4gfkjjdwqx5nsi4a
{
  ALTER TYPE default::Contact {
      ALTER PROPERTY date_of_birth {
          SET TYPE cal::local_date USING (<cal::local_date>.date_of_birth);
      };
  };
  CREATE SCALAR TYPE default::Roles EXTENDING enum<Admin, User>;
  ALTER TYPE default::Contact {
      CREATE REQUIRED PROPERTY role: default::Roles {
          SET REQUIRED USING (<default::Roles>.role);
      };
  };
  ALTER TYPE default::Contact {
      DROP PROPERTY user_type;
  };
};
