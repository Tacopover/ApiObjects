# Revit Family Auditor

## About The Project

The Family Auditor for Revit is a plugin that will detect whenever a user is loading in a family. This family will then be parsed by the Family Auditor to see if it passes the rules setup by the user. The plugin comes with the following rules:
- Material count check
- Sub-Category check
- File size check
- etc.
- etc.

The number of rules will be expanded in future releases.

One of the main features of the plugin is that it allows the (admin) user to save the rules to the Revit model. This allows for rules to be passed among users in a workshared environment. There are 2 types of users:
- admin user
- normal user
An admin user is able to modify, enable an save rules. Rules are saved to the document.
A normal user will not be able to modify any rules, only view them. For both users the plugin will parse families that are loaded into the document. If a family does not meet the criteria of the rules the load will be cancelled and the user will be alerted.

<p align="right">(<a href="#readme-top">back to top</a>)</p>


### Installation

To install the plugin you can download the installer from the [Releases] section. There are 2 installers: the first one is for an admin user. Use this one if you want to have full control of the plugin.
The second installer is for users with limited authorization that rely on an admin user to define the rules for them. See the Getting Started section for more info on the difference between the 2 installers.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



## Getting Started / Usage

add some screenies of the plugin here and give more detailed description on how to use it

Use this space to show useful examples of how a project can be used. Additional screenshots, code examples and demos work well in this space. You may also link to more resources.



<p align="right">(<a href="#readme-top">back to top</a>)</p>


## Roadmap

- [ ] Add more rules
- [ ] Revit 2025 support
- [ ] Allow user to save settings as default
- [ ] Separate the rules logic from the UI
- [ ] Apply rules when copy pasting between models
- [ ] ...


<p align="right">(<a href="#readme-top">back to top</a>)</p>


## Known bugs

These are some of the currently know bugs that should be resolved some time in the future.
- The TypeUpdater might crash when copy pasting a lot of families at once between projects
- The user input in the UI needs to be more robust in terms of inputs it should accept and handle
- ...


## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<p align="right">(<a href="#readme-top">back to top</a>)</p>


## License

Distributed under the MIT License. See `LICENSE.txt` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



## Contact

Arjan Noya - [LinkedIn](https://www.linkedin.com/in/arjan-noya-53020556/) - arjannoya@apiobjects.nl

Taco Pover [LinkedIn](https://www.linkedin.com/in/taco-pover-25702a25/) - taco@mepover.com

Project Link: [https://github.com/Tacopover/ApiObjects/tree/main](https://github.com/Tacopover/ApiObjects/tree/main) 

<p align="right">(<a href="#readme-top">back to top</a>)</p>



## Acknowledgments

* []()
* []()
* []()

<p align="right">(<a href="#readme-top">back to top</a>)</p>



