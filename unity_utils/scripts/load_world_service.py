import rospy
import sys
import os
import xml.etree.ElementTree as ET


from geometry_msgs.msg import Pose
from nav_msgs.srv import GetMap


from unity_msgs.srv import LoadWorld, LoadWorldRequest


UNITY_ROS_NAVIGATION = "UNITY_ROS_NAVIGATION"


def write_world_file_to_unity_dir(world_file_path, world_urdf):
    with open(world_file_path, "w") as file:
        file.write(world_urdf)
        file.close()


def delete_world_file_in_unity_dir(world_file_path):
    try:
        os.remove(world_file_path)
    except:
        rospy.logwarn("World file could not be deleted")




def get_models_from_world_file(world_file_path):
    root = ET.parse(world_file_path).getroot()

    world = root[0]

    return world.findall("include")


def check_has_ground_plane(world_file_path):
    """
        Checks for a ground plane and if so, returns the initial pose
    """
    includes = get_models_from_world_file(world_file_path)

    for include in includes:
        uri = include.find("uri")

        print("URI", uri, uri.text, "model://ground_plane" in uri.text, not uri)

        if uri == None or not ("model://ground_plane" in uri.text):
            continue

        print("BEHIND")

        pose = include.find("pose")

        try:
            pose = [float(t) for t in pose.text.split(" ")]
        except:
            pose = [0.0] * 6

        return True, pose

    return False, []


def get_static_map_data():
    rospy.wait_for_service("static_map")

    static_map_srv = rospy.ServiceProxy("static_map", GetMap)

    map = static_map_srv().map

    height = map.info.height * map.info.resolution
    width = map.info.width * map.info.resolution

    return width, height, map.info.origin


if __name__ == "__main__":
    print("STARTING NODE")

    rospy.init_node("world_loader")

    rospy.wait_for_service("/load_world")

    world_urdf = rospy.get_param("world_urdf", False)
    world_name = rospy.get_param("world", "world")
    world_file_path = rospy.get_param("world_file_path")

    if not world_urdf or not world_file_path:
        print("No world urdf received")
        rospy.signal_shutdown("No world urdf received")
        sys.exit()

    ## Write URDF to unity file path 
    urdf_file_path = os.path.join(os.environ[UNITY_ROS_NAVIGATION], f"{world_name}.urdf")

    width, height, origin = get_static_map_data()

    write_world_file_to_unity_dir(urdf_file_path, world_urdf)


    load_world_srv = rospy.ServiceProxy("/load_world", LoadWorld)

    srv_msg = LoadWorldRequest()
    srv_msg.world_path = urdf_file_path
    srv_msg.world_name = world_name

    ## Check for ground Plane
    ground_plane_exists, ground_plane_pose = check_has_ground_plane(world_file_path)

    srv_msg.create_ground_plane = ground_plane_exists

    srv_msg.origin = [origin.position.x, origin.position.y, origin.position.z]

    if ground_plane_exists:
        x, y, z, a_x, a_y, a_z = ground_plane_pose
        pose = Pose()
        pose.position.x = x
        pose.position.y = y
        pose.position.z = z

        srv_msg.pose = pose
        srv_msg.width = width
        srv_msg.height = height

    load_world_srv(srv_msg)

    delete_world_file_in_unity_dir(urdf_file_path)