CREATE MIGRATION m1aywmi4rl6mwl5gewzn3laaq22lswwlvjlgene2c4g3mrhl2vlrka
    ONTO m1p257htobwser3kfmhhxwsc3obi7fd6heqd6vwnc75j6ubr4u52tq
{
  ALTER TYPE default::Contact {
      ALTER PROPERTY marriage_status {
          SET TYPE std::bool USING (<std::bool>.marriage_status);
      };
  };
};
