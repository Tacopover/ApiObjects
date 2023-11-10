Date created: 2023-10-17 13:53
Date last modified: 2023-10-17 13:53

## Plugin description
A plugin that will detect when the user is loading in a new family. Before loading in that family the plugin will check if that family satisfies all the user demands for a family. The user will be able to enable/disable a bunch of rules:

Rules to consider:
- shared parameter check
	- Check all the shared parameters in a family and see if those parameters are in the currently loaded shared parameter file
- host check
	- Let the user choose which host types to allow in the project:
		- Level based
		- Face based
		- Wall based
		- Floor based
		- Ceiling based
		- Roof based
		- Line based
		- Pattern Based? Gotta check this one
- orientation check
	- Find out in which quadrant the family is modeled by checking the location of all the geometry. User should be able to apply this to specific categories, because the requirements for family orientation can differ per category
- shared nested families check
	- Check the number of nested families and the levels of nesting. This could mean opening multiple nested documents in the background, not sure if this is possible
- number of solids check
- import cad files check
- Sub-category check
	- Check if every piece of geometry is assigned to a sub-category
- Connector check
	- check if connector parameters are associated to family parameters
	- in the case of fittings with more than 1 connector: are there linked connectors in the family?
- Material check (I've seen families with 20000+ materials in them)
- check if the family will create a duplicate (when you get a duplicate family with '1' suffix)
	- Not sure on how to do this.... Would be cool if we could do this

## Features:
- Admin users should be able to enable or disable rules.
- These rules have to be passed to normal users. Perhaps this can be done by using Extensible storage across a Workshared model (this plugin will most likely only be used on big projects with multiple users in a single model). Give every rule an index number so that you only have to store index numbers as key and True/False as value. Store it in the project information of the model. Of course the plugin needs to be installed by every user working on the same model

## UI
[[FamilyLoader UI.excalidraw]] 