using Bronya.Entities;
using Bronya.Maps.NHibMaps;

public class RulesTextMap : NHSubclassClassMap<RulesText>
{
    public RulesTextMap()
    {
        Map(x => x.RuleText)
            .Length(5000);
    }
}