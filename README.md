HexSolver <i><small><sup>(an Hexcells solver)<sup></small></i>
==============================================================

An automatic solver for [Hexcells](http://www.matthewbrowngames.com/hexcells.html), [Hexcells Plus](http://www.matthewbrowngames.com/hexcellsplus.html) and [Hexcells Infinite](http://www.matthewbrowngames.com/hexcellsinfinite.html).  
The idea is to automatically parse the game state, find the next (valid) step and execute it.  
*(Rinse and Repeat until everything is solved)*

### [> Current Release](http://gfycat.com/GrotesqueRecklessAcornbarnacle)

### [> Animation](https://github.com/Mikescher/HexSolver/releases)

##Usage

 - Start HexCells Infinite *(should also work with the other Hexcell games)*
 - I recommend window-mode with 1440x900 resolution (for the OCR to work best)
 - Load a level
 - Start HexSolver
 - Press **Recapture**
 - If you want to completely solve the level press **Execute (All)**
 - Don't manually move your mouse until finished (press ESC to abort)
 - If you just want to see the next step press **Solve** (Can take around 5-10 seconds)

##Troubleshooting

 - HexSolver needs an minimum amount of orange cells to recognize the layout
 - HexSolver only works when all cells are in an uniform grid (click **Calculate** to see the grid)
 - Only click Recapture when the fading in effect is finished - otherwise no cells can be recognized
 - If you find the (uncommon) case of two row-hint in one cell, HexSolver will fail *<sup>(sorry)</sup>*
 - If HexSolver fails to solve a configuration or the OCR module fails, please send me an <u>full-resolution</u> screenshot of the game.

##Features

 - Automatic finding of game window and capturing of its graphical output
 - Dynamically finding the hexagon layout
 - With an custom crafted OCR module recognition of the cell values
 - 3-Step solving of the current configuration (tested on the original levels and many of the generated ones)
 - Finding the optimal execution path by solving the corresponding [TSP](http://en.wikipedia.org/wiki/Travelling_salesman_problem)
 - Automatic execution by programmatically moving the mouse

##HexSolvers internal steps

###Step 1 - Capture

![Shot1](https://raw.githubusercontent.com/Mikescher/HexSolver/master/README-FILES/shot1.png)
*(Captured Screenshot)*

###Step 2 - Find Hexagons

![Shot2](https://raw.githubusercontent.com/Mikescher/HexSolver/master/README-FILES/shot2.png)
*(Binarizing Image to find hexagons)*

![Shot3](https://raw.githubusercontent.com/Mikescher/HexSolver/master/README-FILES/shot3.png)
*(Putting Hexagon Grid over screenshot)*

###Step 3 - Recognize Types

![Shot4](https://raw.githubusercontent.com/Mikescher/HexSolver/master/README-FILES/shot4.png)
*(Find the layout and the cell types)*

###Step 4 - Image Processing

![Shot5](https://raw.githubusercontent.com/Mikescher/HexSolver/master/README-FILES/shot5.png)
*(Find the cells with numbers and extract them)*

###Step 5 - Text Recognition

![Shot6](https://raw.githubusercontent.com/Mikescher/HexSolver/master/README-FILES/shot6.png)
*(OCR the numbers with my own HexCells-OCR engine)*

![Shot7](https://raw.githubusercontent.com/Mikescher/HexSolver/master/README-FILES/shot7.png)
*(the OCR distance of the different numbers)*

###Step 6 - Puzzle Solving

![Shot8](https://raw.githubusercontent.com/Mikescher/HexSolver/master/README-FILES/shot8.png)
*(Show all the current active hints)*

![Shot9](https://raw.githubusercontent.com/Mikescher/HexSolver/master/README-FILES/shot9.png)
*(Find solutions for the current configuration)*

![Shot10](https://raw.githubusercontent.com/Mikescher/HexSolver/master/README-FILES/shot10.png)
*(Find an optimal execution path by TSP algorithm)*

###Step 7 - Solution Executing

####[>>> Animation <<<](http://gfycat.com/GrotesqueRecklessAcornbarnacle)