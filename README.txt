README
CogAssess 1.6.1
Austin Byers, 2013


Overview
-------------------------
CogAssess is a Visual C# program that was sponsored by the University of Chicago for research purposes. In particular, UChicago is involved in a research project that is studying how people value health insuracne in certain areas of southern India. The purpose of the tests is to provide a simple intelligence assessment for each of the 12,000 research participants.

The program consists of four cognitive assessments:

	- Raven's Progressive Matrices (measuring fluid intelligence)
	- coding speed test (measuring processing speed)
	- Corsi block test (measuring short term memory span)
	- numeric Stroop test (measuring cognitive control)

The version used in the field saves to a SQL database and includes a translated Kannada build. For demonstration purposes, the version included here is in English, and saves to local text files.

Although CogAssess was developed for a particular research project, these are all standard cognitive assessments that can be taken by anyone!

The code for the program is posted with permission from the University of Chicago and its research partners. It can be used, copied and modified for any use (including for other research projects), just please give credit where credit is due!


Program Use
-------------------------
To use the program, just run the included "CogAssess 1.6.1.exe" file.
Be sure to read the instructions on the first screen, paying attention to which key combinations you can use during the test to advance, minimize, and quit the tests. For example, there a several "splash screens" which can only be advanced by double-tapping the enter key.

When you're ready, enter your name to begin. After the keyboard training, you will go through all 4 tests in a random order. Instructions for taking each of the tests are as follows:

	- Raven: choose which option fits the best into the grid
	- Speed: The top grid shows several pairs of numbers and symbols, assigned completely at random. Choose the 4-digit number that matches the 4 symbols displayed above the buttons
    - Corsi: the program will highlight a random sequence of squares. When it is done, click on the squares in the same order that they were highlighted in, and press the arrow to continue.
    - Stroop: Enter the number of digits you see on the screen. Sometimes this will be congruent (i.e. "333" has 3 digits) and other times it will be incongruent (i.e. "55" has 2 digits)


Test Results
-------------------------
Each of the tests saves a separate text file in CSV format. For an explanation of the results, click the "Data Files Help" button on the main program screen


C# Code
-------------------------
Open "CognitiveAssessmentsC.sln" in Visual Studio to view the code. Interesting snippets include:

	- an implementation of the Fisher-Yates shuffle algorithm
	- Model-View Controller design - test calculations are encapsulated in separate classes,
		which isolates them from the user interaction routines
	- lots of timers and stopwatches 
	- all form controls are modified at run time to fit the screen
		(guaranteed to work with screen resolutions down to 800 x 600)
	- about 3,800 lines of clean, commented code!