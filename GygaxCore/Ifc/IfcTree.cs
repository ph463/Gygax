using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;
using SharpDX;

namespace GygaxCore.Ifc
{
    public class IfcTree
    {
        private long _ifcModel = 0;

        enum HeaderItem
        {
            Description,
            ImplementationLevel,
            Name,
            TimeStamp,
            Author,
            Organization,
            PreprocessorVersion,
            OriginatingSystem,
            Authorization,
            FileSchemas
        };

        public TreeNode<TreeElement> _ifcTree = new TreeNode<TreeElement>(new TreeElement()
        {
            Key = "Ifc"
        });

        public TreeNode<TreeElement> GetIfcTree(long _ifcModel)
        {
            this._ifcModel = _ifcModel;
            ReadHeaderTreeItems();
            ReadProjectTreeItems();

            return _ifcTree;
        }

        private TreeElement ReadSingleValue(HeaderItem headerItem, string key)
        {
            IntPtr ptr;

            int i = 0;

            IfcEngine.x64.GetSPFFHeaderItem(_ifcModel, (long) headerItem, i++, IfcEngine.x64.sdaiSTRING, out ptr);

            return new TreeElement
            {
                Key = key,
                Value = Marshal.PtrToStringAnsi(ptr),
                //GlobalId = getGlobalId((long) headerItem)
            };
        }

        private TreeNode<TreeElement> ReadMultipleValues(HeaderItem headerItem, string key, string nodeName)
        {
            IntPtr ptr;

            TreeNode<TreeElement> elements = new TreeNode<TreeElement>(new TreeElement()
            {
                Key = nodeName
            });

            int i = 0;

            while (IfcEngine.x64.GetSPFFHeaderItem(_ifcModel, (long)headerItem, i++, IfcEngine.x64.sdaiSTRING, out ptr) == 0)
            {
                elements.AddChild(new TreeElement()
                {
                    Key = key,
                    Value = Marshal.PtrToStringAnsi(ptr)
                });
            }

            return elements;
        }

        private void ReadHeaderTreeItems()
        {
            var header = _ifcTree.AddChild(new TreeElement
            {
                Key = "Header Info"
            });

            header.AddChildren(ReadMultipleValues(HeaderItem.Description, HeaderItem.Description.ToString(),
                "Set of " + HeaderItem.Description.ToString() + "s"));

            header.AddChildren(ReadSingleValue(HeaderItem.ImplementationLevel, HeaderItem.ImplementationLevel.ToString()));
            header.AddChildren(ReadSingleValue(HeaderItem.Name, HeaderItem.Name.ToString()));
            header.AddChildren(ReadSingleValue(HeaderItem.TimeStamp, HeaderItem.TimeStamp.ToString()));

            header.AddChildren(ReadMultipleValues(HeaderItem.Author, HeaderItem.Author.ToString(),
                "Set of " + HeaderItem.Author.ToString() + "s"));
            header.AddChildren(ReadMultipleValues(HeaderItem.Organization, HeaderItem.Organization.ToString(),
                "Set of " + HeaderItem.Organization.ToString() + "s"));

            header.AddChildren(ReadSingleValue(HeaderItem.PreprocessorVersion, HeaderItem.PreprocessorVersion.ToString()));
            header.AddChildren(ReadSingleValue(HeaderItem.OriginatingSystem, HeaderItem.OriginatingSystem.ToString()));
            header.AddChildren(ReadSingleValue(HeaderItem.Authorization, HeaderItem.Authorization.ToString()));
            header.AddChildren(ReadSingleValue(HeaderItem.FileSchemas, HeaderItem.FileSchemas.ToString()));

        }

        private void ReadProjectTreeItems()
        {
            var project = _ifcTree.AddChild(new TreeElement
            {
                Key = "Project Info"
            });

            long iEntityID = IfcEngine.x64.sdaiGetEntityExtentBN(_ifcModel, "IfcProject");
            long iEntitiesCount = IfcEngine.x64.sdaiGetMemberCount(iEntityID);

            for (int iEntity = 0; iEntity < iEntitiesCount; iEntity++)
            {
                long iInstance = 0;
                IfcEngine.x64.engiGetAggrElement(iEntityID, iEntity, IfcEngine.x64.sdaiINSTANCE, out iInstance);

                var child = project.AddChild(CreateTreeItem(iInstance));
                GetTreeItems(iInstance, "IfcSite", ref child);
            }
        }

        private void GetTreeItems(long iParentInstance, string strEntityName, ref TreeNode<TreeElement> tree)
        {
            // check for decomposition
            IntPtr decompositionInstance;
            IfcEngine.x64.sdaiGetAttrBN(iParentInstance, "IsDecomposedBy", IfcEngine.x64.sdaiAGGR, out decompositionInstance);

            if (decompositionInstance == IntPtr.Zero)
            {
                return;
            }

            long iDecompositionsCount = IfcEngine.x64.sdaiGetMemberCount(decompositionInstance.ToInt64());
            for (int iDecomposition = 0; iDecomposition < iDecompositionsCount; iDecomposition++)
            {
                long iDecompositionInstance = 0;
                IfcEngine.x64.engiGetAggrElement(decompositionInstance.ToInt64(), iDecomposition,
                    IfcEngine.x64.sdaiINSTANCE, out iDecompositionInstance);

                if (!IsInstanceOf(iDecompositionInstance, "IFCRELAGGREGATES"))
                {
                    continue;
                }

                IntPtr objectInstances;
                IfcEngine.x64.sdaiGetAttrBN(iDecompositionInstance, "RelatedObjects", IfcEngine.x64.sdaiAGGR,
                    out objectInstances);

                long iObjectsCount = IfcEngine.x64.sdaiGetMemberCount(objectInstances.ToInt64());
                for (int iObject = 0; iObject < iObjectsCount; iObject++)
                {
                    long iObjectInstance = 0;
                    IfcEngine.x64.engiGetAggrElement(objectInstances.ToInt64(), iObject, IfcEngine.x64.sdaiINSTANCE,
                        out iObjectInstance);

                    if (!IsInstanceOf(iObjectInstance, strEntityName))
                    {
                        continue;
                    }

                    var child = tree.AddChild(CreateTreeItem(iObjectInstance));

                    switch (strEntityName)
                    {
                        default:
                        {
                            Console.WriteLine("0");
                        }
                            break;
                        case "IfcSite":
                        {
                            GetTreeItems(iObjectInstance, "IfcBuilding", ref child);
                        }
                            break;

                        case "IfcBuilding":
                        {
                            GetTreeItems(iObjectInstance, "IfcBuildingStorey", ref child);
                        }
                            break;

                        case "IfcBuildingStorey":
                        {
                            
                        //    long imDecompositionInstance = 0;
                        //    IfcEngine.x64.engiGetAggrElement(iObjectInstance, iDecomposition,
                        //        IfcEngine.x64.sdaiINSTANCE, out imDecompositionInstance);
                        //    GetTreeItems(imDecompositionInstance, "IfcAssembly", ref child);
                        //    }
                        //    break;

                        //case "IfcAssembly":
                        //    {
                                AddElementTreeItems(iObjectInstance, ref child);

                            }
                            break;
                            
                    }
                }
            }
        }

        private TreeElement CreateTreeItem(long instance)
        {
            long entity = IfcEngine.x64.sdaiGetInstanceType(instance);
            IntPtr entityNamePtr = IntPtr.Zero;
            IfcEngine.x64.engiGetEntityName(entity, IfcEngine.x64.sdaiSTRING, out entityNamePtr);
            string strIfcType = Marshal.PtrToStringAnsi(entityNamePtr);

            IntPtr name;
            IfcEngine.x64.sdaiGetAttrBN(instance, "Name", IfcEngine.x64.sdaiSTRING, out name);

            string strName = Marshal.PtrToStringAnsi(name);

            IntPtr description;
            IfcEngine.x64.sdaiGetAttrBN(instance, "Description", IfcEngine.x64.sdaiSTRING, out description);

            string strDescription = Marshal.PtrToStringAnsi(description);

            IntPtr gvalue = IntPtr.Zero;
            IfcEngine.x64.sdaiGetAttrBN(instance, "GlobalId", IfcEngine.x64.sdaiSTRING, out gvalue);

            string globalId = Marshal.PtrToStringAnsi((IntPtr) gvalue);

            string strItemText = "'" + (string.IsNullOrEmpty(strName) ? "<name>" : strName) +
                                 "', '" + (string.IsNullOrEmpty(strDescription) ? "<description>" : strDescription) +
                                 "' (" + strIfcType + ")";

            return new TreeElement()
            {
                Key = strName,
                Value = strDescription + " (" + strIfcType + ")",
                GlobalId = globalId
            };
        }

        private bool IsInstanceOf(long iInstance, string strType)
        {
            if (IfcEngine.x64.sdaiGetInstanceType(iInstance) == IfcEngine.x64.sdaiGetEntity(_ifcModel, strType))
            {
                return true;
            }

            return false;
        }

        private void AddElementTreeItems(long iParentInstance, ref TreeNode<TreeElement> tree)
        {
            IntPtr decompositionInstance;
            IfcEngine.x64.sdaiGetAttrBN(iParentInstance, "IsDecomposedBy", IfcEngine.x64.sdaiAGGR,
                out decompositionInstance);

            if (decompositionInstance == IntPtr.Zero)
            {
                return;
            }

            long iDecompositionsCount = IfcEngine.x64.sdaiGetMemberCount(decompositionInstance.ToInt64());
            for (int iDecomposition = 0; iDecomposition < iDecompositionsCount; iDecomposition++)
            {
                long iDecompositionInstance = 0;
                IfcEngine.x64.engiGetAggrElement(decompositionInstance.ToInt64(), iDecomposition,
                    IfcEngine.x64.sdaiINSTANCE, out iDecompositionInstance);

                if (!IsInstanceOf(iDecompositionInstance, "IFCRELAGGREGATES"))
                {
                    continue;
                }

                IntPtr objectInstances;
                IfcEngine.x64.sdaiGetAttrBN(iDecompositionInstance, "RelatedObjects", IfcEngine.x64.sdaiAGGR,
                    out objectInstances);

                long iObjectsCount = IfcEngine.x64.sdaiGetMemberCount(objectInstances.ToInt64());
                
                for (int iObject = 0; iObject < iObjectsCount; iObject++)
                {
                    long iObjectInstance = 0;
                    IfcEngine.x64.engiGetAggrElement(objectInstances.ToInt64(), iObject, IfcEngine.x64.sdaiINSTANCE,
                        out iObjectInstance);

                    tree.AddChild(CreateTreeItem(iObjectInstance));
                }
            }

            // check for elements
            IntPtr elementsInstance;
            IfcEngine.x64.sdaiGetAttrBN(iParentInstance, "ContainsElements", IfcEngine.x64.sdaiAGGR,
                out elementsInstance);

            if (elementsInstance == IntPtr.Zero)
            {
                return;
            }

            long iElementsCount = IfcEngine.x64.sdaiGetMemberCount(elementsInstance.ToInt64());
            for (int iElement = 0; iElement < iElementsCount; iElement++)
            {
                long iElementInstance = 0;
                IfcEngine.x64.engiGetAggrElement(elementsInstance.ToInt64(), iElement, IfcEngine.x64.sdaiINSTANCE,
                    out iElementInstance);

                if (!IsInstanceOf(iElementInstance, "IFCRELCONTAINEDINSPATIALSTRUCTURE"))
                {
                    continue;
                }
                
                //IntPtr objectInstances2;
                //IfcEngine.x64.sdaiGetAttrBN(iElementInstance, "IsDecomposedBy", IfcEngine.x64.sdaiAGGR,
                //out objectInstances2);

                //long iObjectsCount2 = IfcEngine.x64.sdaiGetMemberCount(objectInstances2.ToInt64());

                IntPtr objectInstances;
                IfcEngine.x64.sdaiGetAttrBN(iElementInstance, "RelatedElements", IfcEngine.x64.sdaiAGGR,
                    out objectInstances);

                long iObjectsCount = IfcEngine.x64.sdaiGetMemberCount(objectInstances.ToInt64());
                for (int iObject = 0; iObject < iObjectsCount; iObject++)
                {
                    long iObjectInstance = 0;
                    IfcEngine.x64.engiGetAggrElement(objectInstances.ToInt64(), iObject, IfcEngine.x64.sdaiINSTANCE,
                        out iObjectInstance);

                    var child = tree.AddChild(CreateTreeItem(iObjectInstance));
                    AddElementTreeItems(iObjectInstance, ref child);


                    IntPtr definedByInstances;
                    IfcEngine.x64.sdaiGetAttrBN(iObjectInstance, "IsDefinedBy", IfcEngine.x64.sdaiAGGR,
                        out definedByInstances);


                    if (definedByInstances == IntPtr.Zero)
                    {
                        continue;
                    }

                    long iDefinedByCount = IfcEngine.x64.sdaiGetMemberCount(definedByInstances.ToInt64());
                    for (int iDefinedBy = 0; iDefinedBy < iDefinedByCount; iDefinedBy++)
                    {
                        long iDefinedByInstance = 0;
                        IfcEngine.x64.engiGetAggrElement(definedByInstances.ToInt64(), iDefinedBy,
                            IfcEngine.x64.sdaiINSTANCE, out iDefinedByInstance);

                        if (IsInstanceOf(iDefinedByInstance, "IFCRELDEFINESBYPROPERTIES"))
                        {
                            AddPropertyTreeItems(iDefinedByInstance, ref child);
                        }
                        else
                        {
                            if (IsInstanceOf(iDefinedByInstance, "IFCRELDEFINESBYTYPE"))
                            {

                            }
                        }
                    }
                }
            }
        }

        private void AddPropertyTreeItems(long iParentInstance, ref TreeNode<TreeElement> tree)
        {
            IntPtr propertyInstances;
            IfcEngine.x64.sdaiGetAttrBN(iParentInstance, "RelatingPropertyDefinition", IfcEngine.x64.sdaiINSTANCE,
                out propertyInstances);

            if (IsInstanceOf(propertyInstances.ToInt64(), "IFCELEMENTQUANTITY"))
            {
                var child = tree.AddChild(CreateTreeItem(propertyInstances.ToInt64()));

                // check for quantity
                IntPtr quantitiesInstance;
                IfcEngine.x64.sdaiGetAttrBN(propertyInstances.ToInt64(), "Quantities", IfcEngine.x64.sdaiAGGR,
                    out quantitiesInstance);

                if (quantitiesInstance == IntPtr.Zero)
                {
                    return;
                }

                long iQuantitiesCount = IfcEngine.x64.sdaiGetMemberCount(quantitiesInstance.ToInt64());
                for (int iQuantity = 0; iQuantity < iQuantitiesCount; iQuantity++)
                {
                    long iQuantityInstance = 0;
                    IfcEngine.x64.engiGetAggrElement(quantitiesInstance.ToInt64(), iQuantity, IfcEngine.x64.sdaiINSTANCE,
                        out iQuantityInstance);

                    if (IsInstanceOf(iQuantityInstance, "IFCQUANTITYLENGTH"))
                        child.AddChild(CreatePropertyTreeItem(iQuantityInstance, "IFCQUANTITYLENGTH"));
                    else if (IsInstanceOf(iQuantityInstance, "IFCQUANTITYAREA"))
                        child.AddChild(CreatePropertyTreeItem(iQuantityInstance, "IFCQUANTITYAREA"));
                    else if (IsInstanceOf(iQuantityInstance, "IFCQUANTITYVOLUME"))
                        child.AddChild(CreatePropertyTreeItem(iQuantityInstance, "IFCQUANTITYVOLUME"));
                    else if (IsInstanceOf(iQuantityInstance, "IFCQUANTITYCOUNT"))
                        child.AddChild(CreatePropertyTreeItem(iQuantityInstance, "IFCQUANTITYCOUNT"));
                    else if (IsInstanceOf(iQuantityInstance, "IFCQUANTITYWEIGTH"))
                        child.AddChild(CreatePropertyTreeItem(iQuantityInstance, "IFCQUANTITYWEIGTH"));
                    else if (IsInstanceOf(iQuantityInstance, "IFCQUANTITYTIME"))
                        child.AddChild(CreatePropertyTreeItem(iQuantityInstance, "IFCQUANTITYTIME"));
                    else
                        throw new NotImplementedException();
                }
            }
            else
            {
                if (IsInstanceOf(propertyInstances.ToInt64(), "IFCPROPERTYSET"))
                {
                    var child = tree.AddChild(CreateTreeItem(propertyInstances.ToInt64()));

                    // check for quantity
                    IntPtr propertiesInstance;
                    IfcEngine.x64.sdaiGetAttrBN(propertyInstances.ToInt64(), "HasProperties", IfcEngine.x64.sdaiAGGR,
                        out propertiesInstance);

                    if (propertiesInstance == IntPtr.Zero)
                    {
                        return;
                    }

                    long iPropertiesCount = IfcEngine.x64.sdaiGetMemberCount(propertiesInstance.ToInt64());
                    for (int iProperty = 0; iProperty < iPropertiesCount; iProperty++)
                    {
                        long iPropertyInstance = 0;
                        IfcEngine.x64.engiGetAggrElement(propertiesInstance.ToInt64(), iProperty,
                            IfcEngine.x64.sdaiINSTANCE, out iPropertyInstance);

                        if (!IsInstanceOf(iPropertyInstance, "IFCPROPERTYSINGLEVALUE"))
                            continue;

                        child.AddChild(CreatePropertyTreeItem(iPropertyInstance, "IFCPROPERTYSINGLEVALUE"));
                    }
                }
            }
        }

        /// <summary>
        /// Helper
        /// </summary>
        /// <param name="iParentInstance"></param>
        unsafe Vector3 getRGB_surfaceStyle(int iParentInstance)
        {
            IntPtr stylesInstance;
            IfcEngine.x64.sdaiGetAttrBN(iParentInstance, "Styles", IfcEngine.x64.sdaiAGGR, out stylesInstance);

            Vector3 returnValue = new Vector3();

            long iStylesCount = IfcEngine.x64.sdaiGetMemberCount(stylesInstance.ToInt64());
            for (int iStyle = 0; iStyle < iStylesCount; iStyle++)
            {
                long iStyleInstance = 0;
                IfcEngine.x64.engiGetAggrElement(stylesInstance.ToInt64(), iStyle, IfcEngine.x64.sdaiINSTANCE,
                    out iStyleInstance);

                if (iStyleInstance == 0)
                {
                    continue;
                }

                IntPtr surfaceColour;
                IfcEngine.x64.sdaiGetAttrBN(iStyleInstance, "SurfaceColour", IfcEngine.x64.sdaiINSTANCE,
                    out surfaceColour);

                if (surfaceColour == IntPtr.Zero)
                {
                    continue;
                }

                double R = 0;
                IfcEngine.x64.sdaiGetAttrBN(surfaceColour.ToInt32(), "Red", IfcEngine.x64.sdaiREAL, out *(IntPtr*) &R);

                double G = 0;
                IfcEngine.x64.sdaiGetAttrBN(surfaceColour.ToInt32(), "Green", IfcEngine.x64.sdaiREAL, out *(IntPtr*) &G);

                double B = 0;
                IfcEngine.x64.sdaiGetAttrBN(surfaceColour.ToInt32(), "Blue", IfcEngine.x64.sdaiREAL, out *(IntPtr*) &B);

                returnValue = new Vector3((float) R, (float) G, (float) B);
            }

            return returnValue;
        }

        private TreeElement CreatePropertyTreeItem(long instance, string strProperty)
        {
            //IntPtr ifcType = IfcEngine.x64.engiGetInstanceClassInfo(ifcItem.instance);
            long entity = IfcEngine.x64.sdaiGetInstanceType(instance);
            IntPtr entityNamePtr = IntPtr.Zero;
            IfcEngine.x64.engiGetEntityName(entity, IfcEngine.x64.sdaiSTRING, out entityNamePtr);
            //string strIfcType = Marshal.PtrToStringAnsi(ifcType);
            string strIfcType = Marshal.PtrToStringAnsi(entityNamePtr);

            IntPtr name;
            IfcEngine.x64.sdaiGetAttrBN(instance, "Name", IfcEngine.x64.sdaiSTRING, out name);

            string strName = Marshal.PtrToStringAnsi(name);

            string strValue = string.Empty;
            switch (strProperty)
            {
                case "IFCQUANTITYLENGTH":
                {
                    IntPtr value;
                    IfcEngine.x64.sdaiGetAttrBN(instance, "LengthValue", IfcEngine.x64.sdaiSTRING, out value);

                    strValue = Marshal.PtrToStringAnsi(value);
                }
                    break;

                case "IFCQUANTITYAREA":
                {
                    IntPtr value;
                    IfcEngine.x64.sdaiGetAttrBN(instance, "AreaValue", IfcEngine.x64.sdaiSTRING, out value);

                    strValue = Marshal.PtrToStringAnsi(value);
                }
                    break;

                case "IFCQUANTITYVOLUME":
                {
                    IntPtr value;
                    IfcEngine.x64.sdaiGetAttrBN(instance, "VolumeValue", IfcEngine.x64.sdaiSTRING, out value);

                    strValue = Marshal.PtrToStringAnsi(value);
                }
                    break;

                case "IFCQUANTITYCOUNT":
                {
                    IntPtr value;
                    IfcEngine.x64.sdaiGetAttrBN(instance, "CountValue", IfcEngine.x64.sdaiSTRING, out value);

                    strValue = Marshal.PtrToStringAnsi(value);
                }
                    break;

                case "IFCQUANTITYWEIGTH":
                {
                    IntPtr value;
                    IfcEngine.x64.sdaiGetAttrBN(instance, "WeigthValue", IfcEngine.x64.sdaiSTRING, out value);

                    strValue = Marshal.PtrToStringAnsi(value);
                }
                    break;

                case "IFCQUANTITYTIME":
                {
                    IntPtr value;
                    IfcEngine.x64.sdaiGetAttrBN(instance, "TimeValue", IfcEngine.x64.sdaiSTRING, out value);

                    strValue = Marshal.PtrToStringAnsi(value);
                }
                    break;

                case "IFCPROPERTYSINGLEVALUE":
                {
                    IntPtr value;
                    IfcEngine.x64.sdaiGetAttrBN(instance, "NominalValue", IfcEngine.x64.sdaiSTRING, out value);

                    strValue = Marshal.PtrToStringAnsi(value);
                }
                    break;

                default:
                    throw new Exception("Unknown property.");
            } // switch (strProperty)    

            string strItemText = "'" + (string.IsNullOrEmpty(strName) ? "<name>" : strName) +
                                 "' = '" + (string.IsNullOrEmpty(strValue) ? "<value>" : strValue) +
                                 "' (" + strIfcType + ")";

            //if ((ifcParent != null) && (ifcParent.treeNode != null))
            //{
            //    ifcItem.treeNode = ifcParent.treeNode.Nodes.Add(strItemText);
            //}
            //else
            //{
            //    ifcItem.treeNode = _treeControl.Nodes.Add(strItemText);
            //}

            //if (ifcItem.ifcItem == null)
            //{
            //    // item without visual representation
            //    ifcItem.treeNode.ForeColor = Color.Gray;
            //}

            //ifcItem.treeNode.ImageIndex = ifcItem.treeNode.SelectedImageIndex = IMAGE_PROPERTY;

            return new TreeElement()
            {
                //Key = strItemText
                Key = strName,
                Value = strValue,
                GlobalId = getGlobalId(instance)
            };
        }

        private string getGlobalId(long instance)
        {
            IntPtr gvalue = IntPtr.Zero;
            IfcEngine.x64.sdaiGetAttrBN(instance, "GlobalId", IfcEngine.x64.sdaiSTRING, out gvalue);

            return Marshal.PtrToStringAnsi((IntPtr)gvalue);
        }
    }
    
    public struct TreeElement
    {
        public string Key;
        public string Value;
        public string GlobalId;
        public Vector3 Color;

        public override string ToString()
        {
            if (Value == null || Value.Equals(""))
                return Key;

            return Key + " = " + Value;
        }
    }

    public class TreeNode<T> : ObservableCollection<T>
    {
        private readonly T _value;
        private readonly List<TreeNode<T>> _children = new List<TreeNode<T>>();

        public string Title => _value.ToString();

        public TreeNode(T value)
        {
            _value = value;
        }

        public TreeNode<T> this[int i]
        {
            get { return _children[i]; }
        }

        public TreeNode<T> Parent { get; private set; }

        public T Value { get { return _value; } }

        public ReadOnlyCollection<TreeNode<T>> Children
        {
            get { return _children.AsReadOnly(); }
        }

        public TreeNode<T> AddChild(T value)
        {
            var node = new TreeNode<T>(value) { Parent = this };
            _children.Add(node);
            return node;
        }

        public TreeNode<T>[] AddChildren(params T[] values)
        {
            return values.Select(AddChild).ToArray();
        }

        public TreeNode<T> AddChildren(TreeNode<T> values)
        {
            _children.Add(values);
            return values;
        }

        public bool RemoveChild(TreeNode<T> node)
        {
            return _children.Remove(node);
        }

        public void Traverse(Action<T> action)
        {
            action(Value);
            foreach (var child in _children)
                child.Traverse(action);
        }

        public IEnumerable<T> Flatten()
        {
            return new[] { Value }.Union(_children.SelectMany(x => x.Flatten()));
        }
    }
}
