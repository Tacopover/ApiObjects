# Revit Family Auditor

## About The Project

The Family Auditor for Revit is a plugin that will detect whenever a user is loading in a family. This family will then be parsed by the Family Auditor to see if it passes the rules setup by the user. The plugin comes with the following rules:
- Number of elements check
- Imported instances check
- Material count check
- Sub-Category check
- File size check
- Number of parameters

The number of rules will be expanded in future releases.

One of the main features of the plugin is that it allows the (admin) user to save the rules to the Revit model. This allows for rules to be passed among users in a workshared environment. There are 2 types of users:
- admin user
- normal user

An admin user is able to modify, enable and save rules. Rules are saved to the document.
A normal user will not be able to modify any rules, only view them. For both users the plugin will parse families that are loaded into the document. If a family does not meet the criteria of the rules the load will be cancelled and the user will be alerted.

#### Duplicate families
Another feature of the plugin is that it monitors copy/pasted families from another project. This feature will help to prevent any families with a '1' suffix being added to the model. 
These families get into your model when you paste a Revit family into your project that has the same family name as an existing family, but is slightly different (extra parameter, renamed parameter, whatever). Now when you use Revit's paste functionality from the ribbon, you will get a warning telling you that it has renamed the inserted family. However if you paste into the project by using ctrl+v then Revit will not give you any warning at all and it just pastes the new renamed duplicate family into your project. 
Whenever the plugin detects a duplicate family being pasted into the document it will alert the user with a dialog box. The user can then choose to either rename or replace the existing or new family:
![image](https://github.com/user-attachments/assets/9de01814-ca0f-4bd9-ae62-c10fa0c07ecf)

If the user exits the dialog without choosing hitting the 'OK' button then it will rename the pasted family, which is the default Revit behaviour.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Project setup

Revit plugins need to be maintained for multiple versions so we created a shared project that includes all the codebase and resources.
Each Revit version has each its own seperate project with a different RevitAPI and RevitAPIUI version.
For more information check [this nice blog from archi-lab](https://archi-lab.net/how-to-maintain-revit-plugins-for-multiple-versions-continued/)

## Installation

To install the plugin you can download the installer from the [Releases](https://github.com/Tacopover/ApiObjects/releases) section. There are 2 installers: the first one is for an admin user. Use this one if you want to have full control of the plugin.
The second installer is for users with limited authorization that rely on an admin user to define the rules for them. See the Getting Started section for more info on the difference between the 2 installers.

## Unit Testing

There are a couple of options for unit testing in Revit, we chose RevitTestFramework [(github page)](https://github.com/DynamoDS/RevitTestFramework) for its ease.
It's a console application that works with NUnit. For now we only set up a UnitTest project for Revit 2022 and we will try to make it work for all Revit versions in the future.
The application is executed from a bash file that is included in the RevitTestFrameWork folder.
It will copy all Revit files in that folder to the packages folder where the RTF nuget package is saved.
Finally it will run all the tests that are in the NUnit class and delete the Revit files after testing.

## Getting Started / Usage

After installing the admin plugin it will be possible to enable and modify the rules that will be used when a family is loading into a Revit model. All Rules can be enabled/disabled with the main Enabled/Disabled button.
The 'Save Rules' button will save the rules to the Revit model. When working on a workshared Revit model the rules will also be sent to other users working on the same model via the Revit model after syncing. 
![image](https://github.com/user-attachments/assets/0e3075f1-a6f7-46a9-8cac-85f8457d9fe5)


For a normal user without admin rights there are no buttons to push at all. However you can still see the rules that an admin user has set for the current Revit model:
![image](https://github.com/user-attachments/assets/892c77fb-19d3-47a7-bb4a-9c72dce5deab)



<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Roadmap
Below are some of the milestones on our roadmap

- [ ] Add more rules
- [ ] Revit 2025 support
- [ ] Allow user to save settings as default
- [ ] Separate the rules logic from the UI
- [ ] Apply rules when copy pasting between models
- [ ] ...



## Known bugs

These are some of the currently know bugs that should be resolved some time in the future.
- The TypeUpdater might crash (not Revit ofcourse) when copy pasting a lot of families at once between projects
- The user input in the UI needs to be more robust in terms of inputs it should accept and handle
- We encountered an errormessage when opening the plugin "FileNotFoundException" on one desktop and it has to do with .dll reference. Please contact us if you face the same problem.
You can fix this issue bu manually installing (.addin and .dll).

## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request


## Contact

Arjan Noya - [LinkedIn](https://www.linkedin.com/in/arjan-noya-53020556/) - arjannoya@apiobjects.nl

Taco Pover [LinkedIn](https://www.linkedin.com/in/taco-pover-25702a25/) - taco@mepover.com

Project Link: [https://github.com/Tacopover/ApiObjects/tree/main](https://github.com/Tacopover/ApiObjects/tree/main) 

<p align="right">(<a href="#readme-top">back to top</a>)</p>




