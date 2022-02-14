using ItemChanger.Extensions;
using ItemChanger.Locations;

namespace ItemChanger.Tags
{
    public class DestroyOnECLReplaceTag : Tag
    {
        [Newtonsoft.Json.JsonIgnore]
        private ExistingContainerLocation location;
        public string objectPath;
        public string sceneName;

        public override void Load(object parent)
        {
            location = (ExistingContainerLocation)parent;
            Events.AddSceneChangeEdit(sceneName, OnSceneChange);
        }

        public override void Unload(object parent)
        {
            Events.RemoveSceneChangeEdit(sceneName, OnSceneChange);
        }

        private void OnSceneChange(Scene to)
        {
            if (location.WillBeReplaced())
            {
                UObject.Destroy(to.FindGameObject(objectPath));
            }
        }
    }
}
