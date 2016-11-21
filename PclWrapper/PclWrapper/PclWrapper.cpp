// This is the main DLL file.

#ifdef __cplusplus_cli
#define generic __identifier(generic)
#endif
#include <boost/filesystem.hpp>
#ifdef __cplusplus_cli
#undef generic
#endif

#include "stdafx.h"
#include <string>
#include <iostream>
#include "PclWrapper.h"
#include <pcl/io/pcd_io.h>
#include <pcl/point_types.h>
#include <pcl/filters/voxel_grid.h>

array<PclWrapper::Points>^ PclWrapper::PCD::LoadPointcloud(String^ Name)
{
	//Convert string type
	IntPtr ptr = System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(Name);
	char* str = static_cast<char*>(ptr.ToPointer());

	//Initialize point cloud and read it from file
	pcl::PointCloud<pcl::PointXYZRGB>::Ptr cloud(new pcl::PointCloud<pcl::PointXYZRGB>);

	if (pcl::io::loadPCDFile<pcl::PointXYZRGB>(str, *cloud) == -1) //* load the file
	{
		PCL_ERROR("Couldn't read file test_pcd.pcd \n");
	}

	return convertPointcloudToPoints(cloud);
}

void PclWrapper::PCD::SavePointcloud(char* Name, array<Points>^ cloud)
{
	pcl::io::savePCDFileASCII(Name, *convertPointsToPointcloud(cloud));
}

array<PclWrapper::Points>^ PclWrapper::PCD::convertPointcloudToPoints(pcl::PointCloud<pcl::PointXYZRGB>::Ptr p)
{
	array<PclWrapper::Points>^ pointcloud = gcnew array<PclWrapper::Points>(p->points.size());

	for (int i = 0; i < pointcloud->Length; ++i)
	{
		pointcloud[i].x = p->points[i].x;
		pointcloud[i].y = p->points[i].y;
		pointcloud[i].z = p->points[i].z;
		pointcloud[i].r = p->points[i].r;
		pointcloud[i].g = p->points[i].g;
		pointcloud[i].b = p->points[i].b;
	}

	return pointcloud;
}

pcl::PointCloud<pcl::PointXYZRGB>::Ptr PclWrapper::PCD::convertPointsToPointcloud(array<PclWrapper::Points>^ p)
{
	pcl::PointCloud<pcl::PointXYZRGB>::Ptr NewPointCloud(new pcl::PointCloud<pcl::PointXYZRGB>);

	NewPointCloud->width = p->Length;
	NewPointCloud->height = 1;
	NewPointCloud->is_dense = false;
	NewPointCloud->points.resize(NewPointCloud->width * NewPointCloud->height);

	for (int i = 0; i < p->Length; i++)
	{
		NewPointCloud->points[i].x = p[i].x;
		NewPointCloud->points[i].y = p[i].y;
		NewPointCloud->points[i].z = p[i].z;
		NewPointCloud->points[i].r = p[i].r;
		NewPointCloud->points[i].g = p[i].g;
		NewPointCloud->points[i].b = p[i].b;
	}

	return NewPointCloud;
}

array<PclWrapper::Points>^ PclWrapper::PCD::Process1(array<Points>^ cloud)
{
	pcl::PointCloud<pcl::PointXYZRGB>::Ptr pointcloud = convertPointsToPointcloud(cloud);

	// Do your processing here

	return convertPointcloudToPoints(pointcloud);
}

array<PclWrapper::Points>^ PclWrapper::PCD::Process2(array<Points>^ cloud)
{
	pcl::PointCloud<pcl::PointXYZRGB>::Ptr pointcloud = convertPointsToPointcloud(cloud);

	// Do your processing here

	return convertPointcloudToPoints(pointcloud);
}

array<PclWrapper::Points>^ PclWrapper::PCD::Process3(array<Points>^ cloud)
{
	pcl::PointCloud<pcl::PointXYZRGB>::Ptr pointcloud = convertPointsToPointcloud(cloud);

	// Do your processing here

	return convertPointcloudToPoints(pointcloud);
}

#pragma unmanaged
void calculateICP(pcl::PointCloud<pcl::PointXYZRGB>::Ptr pointcloud1, pcl::PointCloud<pcl::PointXYZRGB>::Ptr pointcloud2, float* t)
{
	pcl::PointCloud<pcl::PointXYZRGB>::Ptr pointcloud4(new pcl::PointCloud<pcl::PointXYZRGB>());

	//pcl::PCLPointCloud2::Ptr pointcloud3;
	//pcl::PCLPointCloud2 pointcloud3;

	pcl::PCLPointCloud2::Ptr pointcloud3(new pcl::PCLPointCloud2());

	pcl::toPCLPointCloud2(*pointcloud2, *pointcloud3);

	pcl::VoxelGrid<pcl::PCLPointCloud2> sor;
	sor.setInputCloud(pointcloud3);
	sor.setLeafSize(0.01f, 0.01f, 0.01f);

	pcl::PCLPointCloud2::Ptr cloud_filtered(new pcl::PCLPointCloud2());
	sor.filter(*cloud_filtered);

	pcl::fromPCLPointCloud2(*cloud_filtered, *pointcloud4);

	pcl::IterativeClosestPoint<pcl::PointXYZRGB, pcl::PointXYZRGB> icp;
	icp.setInputCloud(pointcloud4);
	icp.setInputTarget(pointcloud1);
	pcl::PointCloud<pcl::PointXYZRGB> Final;
	icp.align(Final);

	Eigen::Matrix4f trans = icp.getFinalTransformation();

	t[0] = trans(0, 0);
	t[1] = trans(1, 0);
	t[2] = trans(2, 0);
	t[3] = trans(3, 0);
	t[4] = trans(0, 1);
	t[5] = trans(1, 1);
	t[6] = trans(2, 1);
	t[7] = trans(3, 1);
	t[8] = trans(0, 2);
	t[9] = trans(1, 2);
	t[10] = trans(2, 2);
	t[11] = trans(3, 2);
	t[12] = trans(0, 3);
	t[13] = trans(1, 3);
	t[14] = trans(2, 3);
	t[15] = trans(3, 3);

	std::cout << "has converged:" << icp.hasConverged() << " score: " <<
		icp.getFitnessScore() << std::endl;
	std::cout << icp.getFinalTransformation() << std::endl;

}
#pragma managed

void PclWrapper::PCD::Process4(array<Points>^ cloud1, array<Points>^ cloud2, array<float>^ transformation )
{
	pcl::PointCloud<pcl::PointXYZRGB>::Ptr pointcloud1 = convertPointsToPointcloud(cloud1);
	pcl::PointCloud<pcl::PointXYZRGB>::Ptr pointcloud2 = convertPointsToPointcloud(cloud2);
	

	pin_ptr<float> p = &transformation[0];
	float *cp = p;

	calculateICP(pointcloud1, pointcloud2, cp);
}
