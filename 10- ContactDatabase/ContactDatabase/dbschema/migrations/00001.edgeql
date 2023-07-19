CREATE MIGRATION m1vcqrsvfhsvynibp573pefbz7lao5shsqgd6v5omswdwdw7dzlvxa
    ONTO initial
{
  CREATE TYPE default::Contact {
      CREATE REQUIRED PROPERTY date_of_birth: std::str;
      CREATE REQUIRED PROPERTY description: std::str;
      CREATE REQUIRED PROPERTY email: std::str;
      CREATE REQUIRED PROPERTY firstname: std::str;
      CREATE REQUIRED PROPERTY lastname: std::str;
      CREATE REQUIRED PROPERTY marriagestatus: std::bool;
      CREATE REQUIRED PROPERTY title: std::str;
  };
};
