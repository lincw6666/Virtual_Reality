/*
SJTU - CS230/CS238: Virtual Reality

Homework 1: Hello Teapot

Tasks:

1. compile and get to run
2. wireframe teapot
3. write mouse and keyboard interaction

	Egemen Ertugrul
	egertu@sjtu.edu.cn

*/

//-----------------------------------------------------------------------------
// includes

#include <stdio.h>
#include <cstdlib>
#include <sdlWrapper.h>
#include <shader.h>
#include <glm\glm.hpp>
#include <glm/gtc/matrix_transform.hpp>
#include <model.h>

#undef main // Needed because SDL defines its own main
#ifdef WIN32
	#include <windows.h>
#endif

//-----------------------------------------------------------------------------
// some global variables

// Relative path to source files
std::string codeDir = "../";

// Relative path to models directory
std::string modelsDir = codeDir + "models/";

// flag to check if render wireframe or filled
bool	bWireframe = true;

// near clipping plane
float	zNear = 1.0;

// far clipping plane
float	zFar = 100000.0;

// Function Declarations
bool handleEvents(SDL_Event & evt, sdlWrapper & sdlContext);

// ****************************************************************************
// ************            Insert your code here                   ************
// ****************************************************************************

// You can use these global variables to start thinking about how to implement mouse movements
// You do not have to use these if you don't want

# define PI	3.14159265358979323846	/* pi */

// parameters for the navigation
glm::vec3	viewerPosition	(0.0, 0.0, 50.0);
glm::vec3	viewerCenter	(0.0, 0.0, 0.0);
glm::vec3	viewerUp		(0.0, 1.0, 0.0);

// rotation values for the navigation
float	navigationRotation[3] = { 0.0, 0.0, 0.0 };

// position of the mouse when pressed
Sint32	mousePressedX = 0, mousePressedY = 0;
float	lastXOffset = 0.0, lastYOffset = 0.0, lastZOffset = 0.0;
// mouse button states
enum MouseState { mouseIdle, leftMouseButtonActive, middleMouseButtonActive, rightMouseButtonActive };
MouseState mouseState = mouseIdle;

// Position of the objects.
glm::vec3	teapotPos(0.0f, 0.0f, 0.0f);

void setMousePressedPos(Sint32 const& x, Sint32 const& y) {
	mousePressedX = x, mousePressedY = y;
}

void setMouseLastOffset(float const& x, float const& y, float const& z) {
	lastXOffset = x, lastYOffset = y, lastZOffset = z;
}

void resetMouseLastOffset() {
	lastXOffset = 0.0f, lastYOffset = 0.0f, lastZOffset = 0.0f;
}

// ****************************************************************************
// ****************************************************************************
// ****************************************************************************


//---------------------------------------------------------------
// main function
//---------------------------------------------------------------

void main(int argc, char **argv) {
	
	sdlWrapper sdlContext(1280, 960, "Hello World", 0, false);
	SDL_Event evt;

	float aspectRatio = (float)sdlContext.getWidth() / (float)sdlContext.getHeight();

	glClearColor(0.2f, 0.3f, 0.3f, 1.0f);
	glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);

	printf("\n\nCS230/CS238: Virtual Reality - HW1\n");
	printf("keys:\n\tw\t- toggle wireframe mode\n\tf\t- toggle fullscreen\n\tesc\t- exit\n\n");
	printf("mouse:\n\tleft button\t- rotation\n\tmiddle button\t- panning\n\tright button\t- zoom in and out\n");

	// Shader class instantiation
	Shader shader("../shaders/vertexShader.glsl", "../shaders/fragShader.glsl");

	// ****************************************************************************
	// ************************* Load Your Models Here ****************************
	// ****************************************************************************
	std::cout << "Loading Models ...";
	Model model(modelsDir + "teapot.obj");
	Model axes(modelsDir + "axes.obj");
	std::cout << " Done!" << std::endl;
	// ****************************************************************************
	// ****************************************************************************
	// ****************************************************************************


	// Main rendering loop
	bool running = true;
	while (running) {

		glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
		
		running = handleEvents(evt, sdlContext);

		glViewport(0, 0, sdlContext.getWidth(), sdlContext.getHeight());

		// Always call this before you attach something to uniform, or initiate any draw calls
		shader.Use();
		
		// ****************************************************************************
		// ************************** Insert your Code here ***************************
		// ****************************************************************************

		// Setup Projection, Model, and View Matricies
		glm::mat4 projMat = glm::perspective(glm::radians(50.0f), aspectRatio, zNear, zFar);
		glm::mat4 modelMat = glm::scale(glm::translate(glm::mat4(1.0f), teapotPos), glm::vec3(10.0f));
		glm::mat4 viewMat = glm::lookAt(viewerPosition, viewerCenter, viewerUp);

		// Attached Projection, Model, and View matricies to the shader
		// In the shader the Proj * View * Model * vertex_coord operation is carried out
		shader.attachToUniform("Proj", projMat);
		shader.attachToUniform("View", viewMat);
		shader.attachToUniform("Model", modelMat);

		axes.Draw(shader);
		model.Draw(shader);

		// ****************************************************************************
		// ****************************************************************************
		// ****************************************************************************
		
		sdlContext.swapbuffer();
	}

		
}

//**************************************************************
// SDL event handler function
//**************************************************************

bool handleEvents(SDL_Event & evt, sdlWrapper & sdlContext) {

	// Poll all events that have occurred
	while (SDL_PollEvent(&evt)) {
		// If Quit ( X in window is pressed)
		if (evt.type == SDL_QUIT) {
			return false;
		}

		// Handle Keyboard events
		if (evt.type == SDL_KEYDOWN) {
			// Quit if escape key is pressed
			if (evt.key.keysym.sym == SDLK_ESCAPE) {
				return false;
			}

			// Toggle Fullscreen
			if (evt.key.keysym.sym == SDLK_f) {
				sdlContext.toggleFullScreen();
			}

			// Toggle Wireframe
			if (evt.key.keysym.sym == SDLK_w) {
				if (bWireframe) {
					glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
					bWireframe = false;
				}
				else {
					glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);
					bWireframe = true;
				}
			}
		}

		// ****************************************************************************
		// ************************** Insert your Code here ***************************
		// ****************************************************************************

		// Handle Mouse Click Events
		if (evt.type == SDL_MOUSEBUTTONDOWN) {
			switch (evt.button.button) {
			case SDL_BUTTON_LEFT:
				mouseState = leftMouseButtonActive;
				break;
			
			case SDL_BUTTON_RIGHT:
				mouseState = rightMouseButtonActive;
				setMousePressedPos(evt.button.x, evt.button.y);
				break;
			
			case SDL_BUTTON_MIDDLE:
				mouseState = middleMouseButtonActive;
				setMousePressedPos(evt.button.x, evt.button.y);
				break;
			}
		}

		if (evt.type == SDL_MOUSEBUTTONUP) {
			switch (evt.button.button) {
			case SDL_BUTTON_LEFT:
				mouseState = mouseIdle;
				break;
			
			case SDL_BUTTON_RIGHT:
				mouseState = mouseIdle;
				resetMouseLastOffset();
				break;
			
			case SDL_BUTTON_MIDDLE:
				mouseState = mouseIdle;
				resetMouseLastOffset();
				break;
			}
		}

		// Handle Mouse Motion Events
		// The if_statement is true if the mouse is moving on the window.
		if (evt.type == SDL_MOUSEMOTION) {
			float tmpXOff = 0.0f, tmpYOff = 0.0f;

			switch (mouseState) {
			case middleMouseButtonActive:
				// Calculate the offset of XY.
				tmpXOff = (float)(evt.motion.x - mousePressedX);
				tmpYOff = (float)(mousePressedY - evt.motion.y);
				// Update object's position.
				teapotPos += glm::vec3(tmpXOff - lastXOffset, tmpYOff - lastYOffset, 0.0f);
				setMouseLastOffset(tmpXOff, tmpYOff, 0.0f);
				break;

			case rightMouseButtonActive:
				// Calculate the offset of X.
				tmpXOff = (float)(mousePressedX - evt.motion.x);
				// Update object's position.
				teapotPos += glm::vec3(0.0f, 0.0f, tmpXOff - lastXOffset);
				setMouseLastOffset(tmpXOff, tmpYOff, 0.0f);
				break;

			case leftMouseButtonActive:
				// Calculate the offset of XY.
				tmpXOff = (float)(evt.motion.x - mousePressedX);
				tmpYOff = (float)(mousePressedY - evt.motion.y);
				break;
			}
		}

		// ****************************************************************************
		// ****************************************************************************
		// ****************************************************************************
	}
	return true;
}

