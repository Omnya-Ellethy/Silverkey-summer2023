CREATE MIGRATION m1kki44dtl4lk7d6a7kk6z6e47w4scq4qasrmqirwe2eydjccmpeea
    ONTO m1aywmi4rl6mwl5gewzn3laaq22lswwlvjlgene2c4g3mrhl2vlrka
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
