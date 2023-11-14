namespace CollabAPIMEP
{
    public class Rule
    {
        public bool IsRuleEnabled { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        private string _userInput;
        public string UserInput
        {
            get
            {
                if (_userInput == null)
                {
                    return "N/A";
                }
                else
                {
                    return _userInput;
                }
            }
            set
            {
                if (_userInput != null)
                {
                    _userInput = value;
                }
            }
        }
        public Rule(string name, string userInput = null)
        {
            Name = name;
            IsRuleEnabled = false;
            _userInput = userInput;
        }
    }
}
