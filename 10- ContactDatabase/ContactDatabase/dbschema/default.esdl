module default {
    scalar type Roles extending enum<Admin, User>;
    type Contact {
        required username: str; 
        required password: str;
        required first_name: str;
        required last_name: str;
        required email: str;
        required title: str;
        required date_of_birth: datetime;
        required marriage_status: bool;
        required description: str;
        required property role: Roles;
      
    }
}


