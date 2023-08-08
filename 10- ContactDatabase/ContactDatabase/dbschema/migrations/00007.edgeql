CREATE MIGRATION m1uwy6xdr34dflpvbe6ohbcfeoc2fkk7gfk6na4gfkjjdwqx5nsi4a
    ONTO m1h5rm5ut7qoamb2w65viebnouz6q2ecpxpyawy6lerxznnj4z7eyq
{
  ALTER TYPE default::Contact {
      CREATE REQUIRED PROPERTY user_type: std::str {
          SET REQUIRED USING (<std::str>'');
      };
  };
};
