CREATE MIGRATION m1s66lrfxndwiqez5ivvy7ngzvjr2iobcgud7caxfe7dv7iz3txvqa
    ONTO m1kki44dtl4lk7d6a7kk6z6e47w4scq4qasrmqirwe2eydjccmpeea
{
  ALTER TYPE default::Contact {
      DROP PROPERTY password;
      DROP PROPERTY username;
  };
};
