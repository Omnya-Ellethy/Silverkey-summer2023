CREATE MIGRATION m1h5rm5ut7qoamb2w65viebnouz6q2ecpxpyawy6lerxznnj4z7eyq
    ONTO m1s66lrfxndwiqez5ivvy7ngzvjr2iobcgud7caxfe7dv7iz3txvqa
{
  ALTER TYPE default::Contact {
      CREATE REQUIRED PROPERTY password: std::str {
          SET REQUIRED USING (<std::str>'1234');
      };
      CREATE REQUIRED PROPERTY username: std::str {
          SET REQUIRED USING (<std::str>__source__.first_name);
      };
  };
};
