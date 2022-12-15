using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

using RosMessageTypes.Unity;
using RosMessageTypes.Geometry;

public class ModelController : MonoBehaviour
{
    Dictionary<string, GameObject> activeModels;

    string spawnRobotServiceName = "unity/spawn_robot";
    string moveModelServiceName = "unity/move_model";
    string deleteModelServiceName = "unity/delete_model";

    // Start is called before the first frame update
    void Start() {
        activeModels = new Dictionary<string, GameObject>();
        
        ROSConnection.GetOrCreateInstance().ImplementService<SpawnRobotRequest, SpawnRobotResponse>(
            spawnRobotServiceName, 
            SpawnRobot
        );
        ROSConnection.GetOrCreateInstance().ImplementService<MoveModelRequest, MoveModelResponse>(
            moveModelServiceName, 
            MoveModel
        );
        ROSConnection.GetOrCreateInstance().ImplementService<DeleteModelRequest, DeleteModelResponse>(
            deleteModelServiceName, 
            DeleteModel
        );
    }

    private SpawnRobotResponse SpawnRobot(SpawnRobotRequest request) {

        string name = request.model_name;
        // string modelNameReference = request.model_name_reference;
        string modelNamespace = request.model_namespace; // TODO
        string modelUrdfPath = request.model_urdf_path;
        float[] additionalData = request.additional_data;

        string robotName = modelNamespace.Replace("/", "");

        PoseMsg initialPose = request.initial_pose;

        GameObject robot = Utils.CreateGameObjectFromUrdfFile(
            modelUrdfPath,
            name
        );

        robot.transform.position = new Vector3(
            (float) -initialPose.position.z, 
            (float) initialPose.position.y,
            (float) initialPose.position.x
        );
        robot.transform.rotation = new Quaternion(
            (float) initialPose.orientation.x,
            (float) initialPose.orientation.y,
            (float) initialPose.orientation.z,
            (float) initialPose.orientation.w
        );

        robot.AddComponent(typeof(Rigidbody));

        GameObject laserLink = FindLaserLink(robot);

        Scan laserScan = laserLink.AddComponent(typeof(Scan)) as Scan;
        laserScan.topicNamespace = modelNamespace;
        laserScan.minAngle = additionalData[0];
        laserScan.maxAngle = additionalData[1];
        laserScan.range = additionalData[3];
        laserScan.numBeans = (int) additionalData[4];

        // TODO
        laserScan.frameId = robotName + "_laser_link";

        Odom odom = robot.AddComponent(typeof(Odom)) as Odom;
        odom.topicNamespace = modelNamespace;
        // TODO
        odom.childFrameId = robotName + "_base_footprint";
        odom.frameId = robotName + "_odom";

        Drive drive = robot.AddComponent(typeof(Drive)) as Drive;
        drive.topicNamespace = modelNamespace;



        activeModels.Add(name, robot);

        Debug.Log("Spawned new Robot with name: " + name);

        return new SpawnRobotResponse(true, "Spawned new Robot with name: " + name);
    }

    private MoveModelResponse MoveModel(MoveModelRequest request) {
        string name = request.model_name;

        Debug.Log("MOVING MODEL");

        if ( ! CheckModelExists(name) ) {
            return new MoveModelResponse(false, "Model with name " + name + " does not exist.");
        }

        PoseMsg pose = request.pose;
        
        GameObject objectToMove = activeModels[name];

        objectToMove.transform.position = new Vector3(
            (float) -pose.position.z, 
            (float) pose.position.y,
            (float) pose.position.x
        );
        objectToMove.transform.rotation = new Quaternion(
            (float) pose.orientation.x,
            (float) pose.orientation.y,
            (float) pose.orientation.z,
            (float) pose.orientation.w
        );

        Debug.Log("Moving Model to: " + objectToMove.transform.position);

        // TODO TWIST

        return new MoveModelResponse(true, "Model moved.");
    }

    private GameObject FindLaserLink(GameObject gameObject) {
        Debug.Log(gameObject.name);
        
        if ( gameObject.name == "laser_link" ) {
            return gameObject;
        }

        foreach ( Transform t in gameObject.transform) {
            GameObject possibleLaserLink = FindLaserLink(t.gameObject);

            if ( possibleLaserLink ) {
                return possibleLaserLink;
            }
        }

        return null;
    }

    private DeleteModelResponse DeleteModel(DeleteModelRequest request) {
        string name = request.model_name;

        if ( ! CheckModelExists(name) ) {
            return new DeleteModelResponse(false, "Model with name " + name + " does not exist.");
        }

        GameObject objectToDelete = activeModels[name];

        Destroy(objectToDelete);

        activeModels.Remove(name);
        
        return new DeleteModelResponse(true, "Model with name " + name + " deleted.");
    }

    private bool CheckModelExists(string model_name) {
        Debug.Log("ALL KEYS:");

        foreach (string key in activeModels.Keys)  
        {  
            Debug.Log("Key: " + key);  
        }  

        return activeModels.ContainsKey(model_name);
    }
}
