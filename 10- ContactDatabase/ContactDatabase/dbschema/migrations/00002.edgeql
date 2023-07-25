CREATE MIGRATION m1p257htobwser3kfmhhxwsc3obi7fd6heqd6vwnc75j6ubr4u52tq
    ONTO m1vcqrsvfhsvynibp573pefbz7lao5shsqgd6v5omswdwdw7dzlvxa
{
  ALTER TYPE default::Contact {
      ALTER PROPERTY firstname {
          RENAME TO first_name;
      };
  };
  ALTER TYPE default::Contact {
      ALTER PROPERTY lastname {
          RENAME TO last_name;
      };
  };
  ALTER TYPE default::Contact {
      CREATE REQUIRED PROPERTY marriage_status: std::str {
          SET REQUIRED USING (<std::str>{});
      };
  };
  ALTER TYPE default::Contact {
      DROP PROPERTY marriagestatus;
  };
};
