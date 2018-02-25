//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class GameEntity {

    public HealthComponent health { get { return (HealthComponent)GetComponent(GameComponentLookup.Health); } }
    public bool hasHealth { get { return HasComponent(GameComponentLookup.Health); } }

    public void AddHealth(float newValue) {
        var index = GameComponentLookup.Health;
        var component = CreateComponent<HealthComponent>(index);
        component.Value = newValue;
        AddComponent(index, component);
    }

    public void ReplaceHealth(float newValue) {
        var index = GameComponentLookup.Health;
        var component = CreateComponent<HealthComponent>(index);
        component.Value = newValue;
        ReplaceComponent(index, component);
    }

    public void RemoveHealth() {
        RemoveComponent(GameComponentLookup.Health);
    }
}

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentMatcherApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public sealed partial class GameMatcher {

    static Entitas.IMatcher<GameEntity> _matcherHealth;

    public static Entitas.IMatcher<GameEntity> Health {
        get {
            if (_matcherHealth == null) {
                var matcher = (Entitas.Matcher<GameEntity>)Entitas.Matcher<GameEntity>.AllOf(GameComponentLookup.Health);
                matcher.componentNames = GameComponentLookup.componentNames;
                _matcherHealth = matcher;
            }

            return _matcherHealth;
        }
    }
}