using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OfficialStarterAssets
{
public class DestroyObjectX : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, 2); // destroy particle after 2 seconds
    }


}
}
