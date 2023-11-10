namespace CollabAPIMEP
{
    public class Rule
    {
        public bool IsRuleEnabled { get; set; }
        public string Name { get; set; }
        public Rule(string name)
        {
            Name = name;
            IsRuleEnabled = false;
        }
    }
}
