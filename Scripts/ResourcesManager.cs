using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesManager
{
    static public Material monitoringRenderTexture;
    static public Material textRenderTexture;
    static public Material callingAllRenderTexture;
    static public Material callingPhoneRenderTexture;
    static public Material emptyScreenTexture;
    public ResourcesManager() {
        monitoringRenderTexture = Resources.Load<Material>("renderers/monitoringRenderScene");
        textRenderTexture = Resources.Load<Material>("renderers/textRenderScene");
        callingPhoneRenderTexture = Resources.Load<Material>("calling/calling_phone");
        callingAllRenderTexture = Resources.Load<Material>("calling/calling_all");
        emptyScreenTexture = Resources.Load<Material>("renderers/screen_black");
        Debug.Log(emptyScreenTexture);
    }
}
