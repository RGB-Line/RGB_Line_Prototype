using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;


[CreateAssetMenu(fileName = "Data", menuName = "RGBLine/Resource_MajorPageType", order = 1)]
public sealed class UIPageType : ScriptableObject
{
    public List<string> UIPages;
}