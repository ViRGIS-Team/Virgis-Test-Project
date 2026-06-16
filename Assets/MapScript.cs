using UnityEngine;
using Unity.Netcode;
using Virgis;
using Project;
using System;
using System.Threading.Tasks;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

public class MapInit : MapInitializePrototype
{

    //References to the Prefabs to be used for Layers
    public GameObject CsvLayer;

    /// <summary>
    /// Load the correct scene into the player
    /// </summary>
    public void Awake()
    {
        Debug.Log("Map awakens");
        if (State.instance == null)
        {
            Debug.Log("instantiate app state");
            Instantiate(appState);
        }
        Debug.Log($"Virgis version : {Application.version}");
        State.instance.MapInitialize = this;
        State.instance.Map = gameObject;
    }

    private new void Start()
    {
        base.Start();
        // Start the Host 
        //
        Debug.Log("Starting Server");
        NetworkManager.Singleton.StartHost();
        //Load the project
        //
        Load(Path.Combine(Application.streamingAssetsPath, LoadOnStartup));
    }

    protected override bool _load(string file)
    {
        Debug.Log("Starting  to load Project File");
            StringBuilder builder = new StringBuilder();
            char[] result;
            using (StreamReader reader = File.OpenText(file))
            {
                result = new char[reader.BaseStream.Length];
                reader.Read(result, 0, (int)reader.BaseStream.Length);
                reader.Close();
            }

            foreach (char c in result)
            {
                builder.Append(c);
            }
            
            GisProject project = JsonConvert.DeserializeObject<GisProject>(builder.ToString());
            string test1 = project.ProjectVersion;
            string test2 = new GisProject().GetVersion();
            
            State.instance.project = project;
            State.instance.InitProj();
            State.instance.project.path = Path.GetDirectoryName(file);
            List<RecordSetPrototype> layers = new();
            project.RecordSets.ForEach(recordSet => layers.Add(recordSet as RecordSetPrototype));

            StartCoroutine(initLayers(layers, OnLoad).AsIEnumerator());
            return true;
    }

    public override VirgisLayer CreateLayer(RecordSetPrototype layer)
    {
        VirgisLayer temp = null;
        Transform map = AppState.instance.Map.transform;
        RecordSet thisLayer = layer as RecordSet;
        switch (thisLayer.DataType)
        {
            case RecordSetDataType.CSV:
                temp = Instantiate(CsvLayer, map).GetComponent<CSVLayer>();
                temp.gameObject.AddComponent<CSVLoader>();
                break;
            default:
                Debug.LogError(thisLayer.DataType.ToString() + " is not known.");
                break;
        }
        return temp;
    }

    public override VirgisFeature AddFeature<T>(T geometry)
    {
        throw new NotImplementedException();
    }

    public override IEnumerator Init(RecordSetPrototype layer)
    {
        throw new NotImplementedException();
    }

    public override Task SubInit(RecordSetPrototype layer)
    {
        throw new NotImplementedException();
    }

    public override void OnLoad()
    {
        State.instance.Project.Complete();
    }

    public override void Add(MoveArgs args)
    {
        throw new NotImplementedException();
    }
}

