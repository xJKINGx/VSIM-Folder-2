#pragma once

/*

	CANDIDATE NUMBER: 840

*/

struct DataInformation;

#include <fstream>;
#include <iostream>;
#include <string>;
#include <vector>;
#include <eigen3/Eigen/Eigen>

// The larger the number, the worse the resolution, but the higher
// the chance to get points within that grid cell
int stepLength = 5;

// The smaller this value is, the closer the points will be together
float mapScale = 0.5f; 
Eigen::Vector3f origoOffset = Eigen::Vector3f(0, 0, 0);

DataInformation findExtremes(std::ifstream& file);
void alterData(const std::string& inFile);

void WriteVertexFile(const std::string& outFilePath, const std::vector<std::vector<Eigen::Vector3f>>& grid,
	const float scaleX, const float scaleY, const float scaleZ);
void WriteIndexFile(const std::string& outFilePath, int xStep, int yStep);