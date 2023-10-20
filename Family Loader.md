Date created: 2023-10-17 13:53
Date last modified: 2023-10-17 13:53

## Plugin description
A plugin that will detect when the user is loading in a new family. Before loading in that family the plugin will check if that family satisfies all the user demands for a family.
Demands to consider:
- shared parameter check
- host check
- orientation check
- shared nested families check
- number of solids check
- import cad files check
- Sub-category check
- Connector check
- Material check (I've seen families with 20000+ materials in them)
- check if the family will create a duplicate (when you get a duplicate family with '1' suffix)