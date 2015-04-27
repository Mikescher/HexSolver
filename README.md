HexSolver <i><small><sup>(an Hexcells solver)<sup></small></i>
==============================================================

An automatic solver for [Hexcells](http://www.matthewbrowngames.com/hexcells.html), [Hexcells Plus](http://www.matthewbrowngames.com/hexcellsplus.html) and [Hexcells Infinite](http://www.matthewbrowngames.com/hexcellsinfinite.html).  
The idea is to automatically parse the game state, find the next (valid) step and execute it.  
*(Rinse and Repeat until everything is solved)*

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

> **NOT IMPLEMENTED**