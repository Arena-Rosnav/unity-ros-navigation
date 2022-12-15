cmake_minimum_required(VERSION 3.0.2)
project(unity_utils)

find_package(catkin REQUIRED rospy)

catkin_package()


catkin_install_python(PROGRAMS
  scripts/load_world_service.py

  DESTINATION ${CATKIN_PACKAGE_BIN_DESTINATION}
)

