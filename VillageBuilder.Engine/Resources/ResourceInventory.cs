namespace VillageBuilder.Engine.Resources
{
    public class ResourceInventory
    {
        private Dictionary<ResourceType, int> _resources;

        public ResourceInventory()
        {
            _resources = new Dictionary<ResourceType, int>();
            foreach (ResourceType type in Enum.GetValues<ResourceType>())
            {
                _resources[type] = 0;
            }
        }

        public void Add(ResourceType type, int amount)
        {
            _resources[type] += amount;
        }

        public bool Remove(ResourceType type, int amount)
        {
            if (_resources[type] >= amount)
            {
                _resources[type] -= amount;
                return true;
            }
            return false;
        }

        public int Get(ResourceType type)
        {
            return _resources.TryGetValue(type, out int amount) ? amount : 0;
        }

        public bool Has(ResourceType type, int amount) => _resources[type] >= amount;

        public Dictionary<ResourceType, int> GetAll() => new Dictionary<ResourceType, int>(_resources);
    }
}