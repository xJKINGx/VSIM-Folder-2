#include "main.h";

/*

	CANDIDATE NUMBER: 840

*/

struct DataInformation
{
	float xMin, xMax;
	float yMin, yMax;
	float zMin, zMax;
	float xRange, yRange, zRange;
};

int main()
{
	alterData("assets/merged.txt");

	return 0;
}

void alterData(const std::string& inFile)
{
	std::ifstream inputFile{ inFile };

	if (!inputFile.is_open())
	{
		std::cout << "Could not open file at: " + inFile << std::endl;
		return;
	}
	
	const DataInformation extremes = findExtremes(inputFile);
	// std::cout for debugging
	std::cout << "xMin: " << extremes.xMin << " | xMax: " << extremes.xMax <<
		" | yMin: " << extremes.yMin << " | yMax: " << extremes.yMax << " | zMin: " <<
		extremes.zMin << " | zMax: " << extremes.zMax << " | xRange: " << extremes.xRange
		<< " | yRange: " << extremes.yRange << " | zRange: " << extremes.zRange << std::endl;

	int xSteps = static_cast<int>(ceil(extremes.xRange / stepLength));
	int ySteps = static_cast<int>(ceil(extremes.yRange / stepLength));
	
	origoOffset = 0.5f * Eigen::Vector3f
	( 
		xSteps*stepLength,
		ySteps * stepLength,
		extremes.zMin + extremes.zMax
	);

	std::cout << "xSteps: " << xSteps << "\nySteps: " << ySteps << std::endl;
	std::cout << "Origo offset:\nx: " << origoOffset.x()
		<< "\ny: " << origoOffset.y()
		<< "\nz: " << origoOffset.z() << std::endl;

	// Assuming unique x-values, there can be multiple y- and z-value for each x-value.
	std::vector<std::vector<std::vector<float>>> gridPoints
	(xSteps, std::vector<std::vector<float>>(ySteps, std::vector<float>()));

	std::vector<std::vector<Eigen::Vector3f>> grid
		(xSteps, std::vector<Eigen::Vector3f>(ySteps, Eigen::Vector3f()));

	std::vector<std::vector<bool>> filledCells(xSteps, std::vector<bool>(ySteps, false));
	
	float x{}, y{}, z{};

	inputFile.clear();
	inputFile.seekg(0);

	while (inputFile >> x >> y >> z)
	{
		int i, j;
		// Finding out in which part of the grid the current point resides
		i = static_cast<int>((x - extremes.xMin) / stepLength);
		j = static_cast<int>((y - extremes.yMin) / stepLength);

		// Checking for out of bounds
		if (i < 0) { i = 0; }
		else if (i >= xSteps)
		{
			i = xSteps - 1;
		}
		if (j < 0) { j = 0; }
		else if (j >= ySteps)
		{
			j = ySteps - 1;
		}

		// Adding the point to the correct grid
		gridPoints[i][j].push_back(z);		
	}

	inputFile.close();

	// Now to calculate the average z-value of all points
	for (int i = 0; i < gridPoints.size(); ++i)
	{
		for (int j = 0; j < gridPoints[0].size(); ++j)
		{
			//std::cout << "bababooey: " << gridPoints[i][j].size() << std::endl;
			float meanValue{};
			// Runs through all z-values for the given gridPoints[i][j] point,
			// afterwards it will add it's z-value to the meanValue for that gridspace
			for (int k = 0; k < gridPoints[i][j].size(); ++k)
			{
				meanValue += gridPoints[i][j][k];
			}
			//std::cout << "bababooey 2" << std::endl;

			// Finding the mean for the gridspace
			if (gridPoints[i][j].empty() == false)
			{
				meanValue /= static_cast<const float&>(gridPoints[i][j].size());
				grid[i][j] = Eigen::Vector3f(i * stepLength, j * stepLength, meanValue) - origoOffset;
				filledCells[i][j] = true;
			}
			else
			{
				grid[i][j] = Eigen::Vector3f(i * stepLength, j * stepLength, meanValue) - origoOffset;
				grid[i][j][2] = 0;
			}

			
		}
	}

	gridPoints.clear();

	for (int i = 0; i < grid.size(); i++)
	{
		for (int j = 0; j < grid[0].size(); j++)
		{
			// If the current cell is not void of values
			if (filledCells[i][j])
			{
				continue;
			}

			int numPoints{};

			for (int xn = i - 1; xn <= i + 1; xn++)
			{
				if (xn < 0 || xn >= xSteps) continue;
				for (int yn = j - 1; yn <= j + 1; yn++)
				{
					if (yn < 0 || yn >= ySteps || !filledCells[xn][yn]) continue;
					grid[i][j][2] += grid[xn][yn].z();
					numPoints++;
				}
			}

			grid[i][j][2] /= numPoints > 0 ? numPoints : 1.0f;
			filledCells[i][j] = true;
		}
	}

	// For debugging
	//for (int i = 0; i < grid.size(); i++)
	//{
	//	for (int j = 0; j < grid[0].size(); j++)
	//	{
	//		std::cout << "X: " << grid[i][j].x() << " | Y: " << grid[i][j].y() << " | Z: " << grid[i][j].z() << std::endl;
	//	}
	//}

	//WriteVertexFile("assets/vertices.txt", grid, 1.0f, 1.0f, 1.0f);
	WriteVertexFile("assets/vertices.txt", grid, 0.5f, 0.5f, 0.5f);

	WriteIndexFile("assets/indices.txt", xSteps, ySteps);
}

DataInformation findExtremes(std::ifstream& file)
{
	// Begin the file search at the start of the file
	file.clear();
	file.seekg(0);

	// Defining some variables
	float xMin, yMin, zMin, x, y, z;
	float xMax, yMax, zMax;

	// Input the first row of information into x, y and z
	file >> x >> y >> z;
	
	// Assume that the initial values are both the minimum and maximum values
	xMax = xMin = x;
	yMax = yMin = y;
	zMax = zMin = z;

	// Run through all the points to find the extreme values of x, y and z
	while (file >> x >> y >> z)
	{
		if (x < xMin)
		{
			xMin = x;
		} 
		else if (x > xMax)
		{
			xMax = x;
		}

		if (y < yMin)
		{
			yMin = y;
		}
		else if (y > yMax)
		{
			yMax = y;
		}

		if (z < zMin)
		{
			zMin = z;
		}
		else if (z > zMax)
		{
			zMax = z;
		}
	}

	std::cout << "Extremes found!" << std::endl;

	// Return then the struct with the information gathered.
	return DataInformation{ xMin, xMax, yMin, yMax, zMin, zMax, 
							xMax - xMin, yMax - yMin, zMax - zMin};

}

void WriteVertexFile(const std::string& outFilePath, const std::vector<std::vector<Eigen::Vector3f>>& grid,
	const float scaleX, const float scaleY, const float scaleZ)
{
	std::ofstream outputFile{ outFilePath };
	if (!outputFile.is_open())
	{
		std::cout << "Could not open output file at: " + outFilePath << std::endl;
		return;
	}

	// Adding the amount of vertices in the grid
	size_t vertexAmount = grid.size() * grid[0].size();
	// Here we're adding a "\n" to get to the next line, from here on we print points
	outputFile << vertexAmount << "\n";

	for (size_t i = 0; i < grid.size(); i++)
	{
		for (size_t j = 0; j < grid[0].size(); j++)
		{
			Eigen::Vector3f point = grid[i][j];
			outputFile << point.x() * scaleX << " " << point.y() * scaleY << " "
				<< point.z() * scaleZ << "\n";
		}
	}

	outputFile.close();

}

// For writing indices and neighbour information
void WriteIndexFile(const std::string& outFilePath, int xStep, int yStep)
{
	std::ofstream outputFile{ outFilePath };

	if (!outputFile.is_open())
	{
		std::cout << "Could not open output index file at: " + outFilePath << std::endl;
		return;
	}
	
	// For storing the values we'll create later
	std::vector<int> indices{};
	std::vector<int> neighbours{};

	const int rowLength = 2 * (yStep - 1);
	const int totalTriangles = rowLength * (xStep - 1);

	for (int i = 0; i < xStep - 1; i++)
	{
		const int trianglesCurrently = 2 * i * (yStep - 1);
		for (int j = 0; j < yStep - 1; j++)
		{
			indices.push_back(j + i * yStep);
			indices.push_back((j + 1) + i * yStep);
			indices.push_back(j + (i + 1) * yStep);

			const int evenTriangle = 2 * (i * (yStep - 1) + j);
			const int oddTriangle = evenTriangle + 1;

			// The potential neighbours
			int T0 = oddTriangle;
			int T1 = evenTriangle - 1;
			int T2 = evenTriangle - trianglesCurrently + 1;

			// a ? b : c essentially means: if a, then b, otherwise c
			T0 = T0 < trianglesCurrently + rowLength ? T0 : -1;
			T1 = T1 > trianglesCurrently ? T1 : -1;
			T2 = T2 > 0 ? T2 : -1;

			neighbours.push_back(T0);
			neighbours.push_back(T1);
			neighbours.push_back(T2);

			// Since a square is made of 2 triangles, we need to repeat this code
			// so that we can add in the other triangle of the current square
			indices.push_back((j + 1) + i * yStep);
			indices.push_back((j + 1) + (i + 1) * yStep);
			indices.push_back(j + (i + 1) * yStep);

			T0 = evenTriangle + rowLength;
			T1 = evenTriangle;
			T2 = oddTriangle - 1;

			T0 = T0 < totalTriangles ? T0 : -1;
			T1 = T1 >= trianglesCurrently ? T1 : -1;
			T2 = T2 < trianglesCurrently + rowLength ? T2 : -1;

			neighbours.push_back(T0);
			neighbours.push_back(T1);
			neighbours.push_back(T2);
		}
	}

	outputFile << totalTriangles << "\n";

	for (int i = 2; i < indices.size(); i += 3)
	{
		outputFile << indices[i - 2] << " " << indices[i - 1] << " " << indices[i] << " "
			<< neighbours[i - 2] << " " << neighbours[i - 1] << " " << neighbours[i] << "\n";
	}

	outputFile.close();
}