class LouBul implements WalkerInterface {
  
  int screenWidth;
  int screenHeight;
  int frame = 0;

  PVector currrentPosision;
  PVector up = new PVector(0, -1);
  PVector down = new PVector(0, 1);
  PVector left = new PVector(-1, 0);
  PVector right = new PVector(1, 0);

  int switchInput = 1;
 
  boolean swapDirection;
  int previusDirection;

  String getName()
  {
    return "Lady Wiive Buller"; //When asked, tell them our walkers name
  }

  PVector getStartPosition(int playAreaWidth, int playAreaHeight)
  {
    //Select a starting position or use a random one.
    float x = (int) random(0, playAreaWidth/2);
    float y = (int) random(0, playAreaHeight);

    //a PVector holds floats but make sure its whole numbers that are returned!
    screenWidth = playAreaWidth;
    screenHeight = playAreaHeight;
    currrentPosision = new PVector((int)x, (int)y);
    return new PVector((int)x, (int)y);
  }

  PVector update()
  {        
    swapDirection = alovidToMove(currrentPosision, screenHeight, screenWidth);
    if (swapDirection)
    {
      if (frame % 5 == 0)
      {
        switchInput = (int)random(0, 4);
      }
    }
    else
    {
        
      switchInput = (int)random(0, 4);      
    }
    frame++;

    switch(switchInput) {
    case 0:
      currrentPosision = currrentPosision.add(left);
      return new PVector(-1, 0);
    case 1:
      currrentPosision = currrentPosision.add(right);
      return new PVector(1, 0);
    case 2:
      currrentPosision = currrentPosision.add(down);
      return new PVector(0, 1);
    default:
      currrentPosision = currrentPosision.add(up);
      return new PVector(0, -1);
    }
    
  }
}


boolean alovidToMove(PVector vector, int height, int width)
{
 if ((vector.y+5) < this.height
    && (vector.x+5) < this.width
    && (vector.x-5) > 0
    && (vector.y-5) > 0)
 {
  return true;
 }
  else return false;
}
