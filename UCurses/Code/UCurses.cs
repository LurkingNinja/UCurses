using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Text;
using UnityEngine.UIElements;
using UnityEditor.VersionControl;
using UnityEngine.TextCore.Text;
using System.Drawing;

//Main singleton UCurses class used for all major functionality.
namespace UCursesInclude
{
    public enum ScreenAlignment { Middle, Left, Right }

    [DefaultExecutionOrder(-900)]
    public class UCurses : MonoBehaviour
    {
        //Sets this object as a singleton.
        private static UCurses InstanceOf;
        public static UCurses Instance
        {
            get
            {
                if (InstanceOf == null)
                {
                    InstanceOf = GameObject.FindAnyObjectByType<UCurses>();
                }
                return InstanceOf;
            }
        }
        void Awake()
        {
            if (InstanceOf != null)
            {
                Destroy(this.gameObject);
            }
            InstanceOf = this;
        }




        private const int upscaleRenderTextureFactor = 4;

        private GameObject[,] _tileIndex;

        [SerializeField] private Vector2Int _gridSize;
        [SerializeField] private Vector2Int _dosScreenResolution;
        [SerializeField] private float _aspectRatio;
        [SerializeField] private ScreenAlignment _alignment;
        [SerializeField] private float _alignmentOffset = 0;
        [SerializeField] private Vector2Int _characterSize;
        [SerializeField] private bool _offsetLine;
        [SerializeField] private FilterMode _screenFilterMode;
        [SerializeField] private FilterMode _characterFilterMode;

        [SerializeField] private GameObject _cameraMainObject;
        [SerializeField] private GameObject _canvasGridObject;
        [SerializeField] private GameObject _canvasViewportObject;
        [SerializeField] private GameObject _gridContainerObject;
        [SerializeField] private GameObject _gridImage;
        [SerializeField] private GameObject _gridTilePrefab;
        [SerializeField] private GameObject _subcanvasPrefab;
        [SerializeField] private RenderTexture _gridRenderTexture;
        [SerializeField] private CharSprite[] _charSprites;




        public Vector2Int GridSize
        {
            get { return _gridSize; }
        }
        public Vector2Int DosScreenResolution
        {
            get { return _dosScreenResolution; }
        }

        public float AspectRatio
        {
            get { return _aspectRatio; }
        }

        public Vector2Int CharacterSize
        {
            get { return _characterSize; }
        }

        public bool OffsetLine
        {
            get { return _offsetLine; }
        }

        public FilterMode ScreenFilterMode
        {
            get { return _screenFilterMode; }
        }

        public FilterMode CharacterFilterMode
        {
            get { return _characterFilterMode; }
        }





        void Start()
        {
            createGrid(_gridSize.x, _gridSize.y);
            _gridImage.GetComponent<RectTransform>().sizeDelta = new Vector2(getHorizontalAspectRatio(Screen.height, _aspectRatio), Screen.height);
            setScreenAlignment(_alignment, _alignmentOffset);
            _gridRenderTexture.filterMode = _screenFilterMode;
        }

        //Converts a string to use Unix style newline characters.
        public string unixString(string stringValue)
        {
            stringValue = stringValue.Replace("\r\n", "\n");
            stringValue = stringValue.Replace('\r', '\n');
            return stringValue;
        }

        //Sets the DOS console screen settings being emulated within Ucurses.
        public void setDosScreenMode(DosScreenMode screenMode)
        {
            _gridSize = screenMode.GridSize;
            _dosScreenResolution = screenMode.DosScreenResolution;
            _aspectRatio = screenMode.AspectRatio;
            _alignment = screenMode.ScreenAlignment;
            _alignmentOffset = screenMode.ScreenOffset;
            _characterSize = screenMode.CharacterSize;
            _offsetLine = screenMode.OffsetLine;
            _screenFilterMode = screenMode.ScreenFilterMode;
            _characterFilterMode = screenMode.CharacterFilterMode;

            resizeRenderTexture(_gridRenderTexture, _dosScreenResolution.x * upscaleRenderTextureFactor, _dosScreenResolution.y * upscaleRenderTextureFactor);
            _canvasGridObject.GetComponent<CanvasScaler>().referenceResolution = _dosScreenResolution;
        }

        //Sets the array of ascii image sprites used for the character grid.
        public void setCharSprites(CharSprite[] charSprites)
        {
            _charSprites = charSprites;
        }

        //Resize a RenderTexture object.
        private void resizeRenderTexture(RenderTexture renderTexture, int width, int height)
        {
            renderTexture.Release();
            renderTexture.width = width;
            renderTexture.height = height;
            renderTexture.Create();
        }

        //Sets the horizontal alignment of the screen
        private void setScreenAlignment(ScreenAlignment alignment, float screenOffset)
        {
            switch (alignment)
            {
                case ScreenAlignment.Middle:
                    _gridImage.GetComponent<RectTransform>().anchoredPosition = new Vector2((Screen.width / 2) - (_gridImage.GetComponent<RectTransform>().rect.width / 2) + screenOffset, _gridImage.GetComponent<RectTransform>().anchoredPosition.y);
                    break;
                case ScreenAlignment.Left:
                    _gridImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(screenOffset, _gridImage.GetComponent<RectTransform>().anchoredPosition.y);
                    break;
                case ScreenAlignment.Right:
                    _gridImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(Screen.width - _gridImage.GetComponent<RectTransform>().rect.width + screenOffset, _gridImage.GetComponent<RectTransform>().anchoredPosition.y);
                    break;
            }
        }

        //Creates a grid tile on the canvas.
        private GameObject createGridTile(float pixPosX, float pixPosY, float pixSizeX, float pixSizeY, string objectName, GameObject subcanvasObject)
        {
            GameObject cSpace = GameObject.Instantiate(_gridTilePrefab);
            cSpace.transform.SetParent(subcanvasObject.transform);
            cSpace.GetComponent<RectTransform>().anchoredPosition = new Vector2(pixPosX, pixPosY);
            cSpace.GetComponent<GridTile>().SizeDelta = new Vector2(pixSizeX, pixSizeY);
            cSpace.name = objectName;
            return cSpace;
        }

        //Creates grid of grid tiles.
        private void createGrid(int xSize, int ySize)
        {
            _tileIndex = new GameObject[xSize, ySize];
            for (int yPos = 0; yPos < ySize; yPos++)
            {
                GameObject subcanvas = GameObject.Instantiate(_subcanvasPrefab);
                subcanvas.transform.SetParent(_gridContainerObject.transform);
                subcanvas.name = "Subcanvas" + yPos.ToString();
                subcanvas.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                subcanvas.GetComponent<RectTransform>().localScale = new Vector2(1, 1);
                subcanvas.GetComponent<RectTransform>().sizeDelta = new Vector3(0, 0, 0);

                for (int xPos = 0; xPos < xSize; xPos++)
                {
                    _tileIndex[xPos, yPos] = createGridTile(_characterSize.x * xPos, -_characterSize.y * yPos, _characterSize.x, _characterSize.y, "Tile_" + (xPos + 1) + "x" + (yPos + 1), subcanvas);
                }
            }
        }

        //Returns the ascii value from the sprite array by its char value. Returns -1 if error.
        private Sprite getSprite(char charValue)
        {
            for (int i = 0; i < _charSprites.Length; i++)
            {
                if (_charSprites[i].Character == charValue)
                {
                    return _charSprites[i].Usprite;
                }
            }
            return null;
        }

        //Returns if a sprite is a UI element by its char value.
        private bool isSpriteUI(char charValue)
        {
            for (int i = 0; i < _charSprites.Length; i++)
            {
                if (_charSprites[i].Character == charValue)
                {
                    return _charSprites[i].IsUI;
                }
            }
            return false;
        }

        //Finds the horizontal resolution with a given aspect ratio.
        private int getHorizontalAspectRatio(int verticalRez, float aspectRatio)
        {
            float resolution = verticalRez * aspectRatio;
            resolution = (int)resolution;
            if (resolution % 2 == 1)
            {
                resolution++;
            }

            return (int)resolution;
        }

        //Returns a ascii value from the charSprites array by its char value. -1 is error.
        public char asciiToChar(int asciiValue)
        {
            if (asciiValue > 0 && asciiValue < _charSprites.Length)
            {
                return _charSprites[asciiValue - 1].Character;
            }
            return ' ';
        }

        //Prints a character with a given color on the character grid.
        public void printCharacter(Vector2Int position, char character, Color32 color)
        {
            _tileIndex[position.x, position.y].GetComponent<GridTile>().CharacterColor = color;
            _tileIndex[position.x, position.y].GetComponent<GridTile>().setCharacterSprite(getSprite(character), _characterSize, _offsetLine, isSpriteUI(character));
        }

        //Prints a rectangle of a character with a given color on the character grid.
        public void printCharactersRectangle(Vector2Int position, char character, Color32 color, int width, int height)
        {
            printCharactersRectangle(position, character, color, width, height, true);
        }

        //prints a rectangle of a character with a given color on the character grid.
        public void printCharactersRectangle(Vector2Int position, char character, Color32 color, int width, int height, bool filled)
        {
            if (filled == true)
            {
                for (int yPos = position.y; yPos < position.y + height; yPos++)
                {
                    for (int xPos = position.x; xPos < position.x + width; xPos++)
                    {
                        if (xPos < _gridSize.x && yPos < _gridSize.y)
                        {
                            printCharacter(new Vector2Int(xPos, yPos), character, color);
                        }
                    }
                }
            }
            else
            {
                for (int xPos = position.x; xPos < position.x + width; xPos++)
                {
                    if (xPos < _gridSize.x)
                    {
                        printCharacter(new Vector2Int(xPos, position.y), character, color);
                        if (position.y + (height - 1) < _gridSize.y)
                        {
                            printCharacter(new Vector2Int(xPos, position.y + (height - 1)), character, color);
                        }
                    }
                }
                for (int yPos = position.y; yPos < position.y + height; yPos++)
                {
                    if (yPos < _gridSize.y)
                    {
                        printCharacter(new Vector2Int(position.x, yPos), character, color);
                        if (position.x + (width - 1) < _gridSize.x)
                        {
                            printCharacter(new Vector2Int(position.x + width - 1, yPos), character, color);
                        }
                    }
                }
            }
        }

        //Prints a string of a characters with a given color on the character grid.
        public void printString(Vector2Int position, string charString, Color32 color)
        {
            charString = unixString(charString);
            int offsetY = 0;
            int offsetX = 0;
            for (int i = 0; i < charString.Length; i++)
            {

                if (charString[i] == '\n')
                {
                    offsetY++;
                    offsetX = 0;
                }
                else
                {
                    printCharacter(new Vector2Int(position.x + offsetX, position.y + offsetY), charString[i], color);
                    offsetX++;
                }
            }
        }

        //Prints a 2D array of characters with a given color on the character grid.
        public void printCharArray(Vector2Int position, char[,] charArray, Color32 color)
        {
            for (int posX = 0; posX < charArray.GetLength(0); posX++)
            {
                for (int posY = 0; posY < charArray.GetLength(1); posY++)
                {
                    if (position.x + posX < _gridSize.x && position.y + posY < _gridSize.y)
                    {
                        printCharacter(new Vector2Int(position.x + posX, position.y + posY), charArray[posX, posY], color);
                    }
                }
            }
        }

        //Prints a 2D array of characters with a corresponding 2D Color32 array on the character grid.
        public void printCharArray(Vector2Int position, char[,] charArray, Color32[,] colorArray)
        {
            for (int posX = 0; posX < charArray.GetLength(0); posX++)
            {
                for (int posY = 0; posY < charArray.GetLength(1); posY++)
                {
                    if (position.x + posX < _gridSize.x && position.y + posY < _gridSize.y)
                    {
                        printCharacter(new Vector2Int(position.x + posX, position.y + posY), charArray[posX, posY], colorArray[posX, posY]);
                    }
                }
            }
        }

        //Prints a whole number with a given color on the character grid.
        public void printNumberInt(Vector2Int position, int numericValue, Color32 color)
        {
            printNumberInt(position, numericValue, color, (uint)numericValue.ToString().Length, false);
        }

        //prints a whole number with a given color on the character grid.
        public void printNumberInt(Vector2Int position, int numericValue, Color32 color, uint paddingLength, bool addZeros)
        {
            string intString = "";
            int toStringLength = numericValue.ToString().Length;
            if (paddingLength - toStringLength >= 0)
            {
                long counter = paddingLength - toStringLength;
                for (int i = 0; i < counter; i++)
                {
                    if (addZeros == true)
                    {
                        intString += "0";
                    }
                    else
                    {
                        intString += " ";
                    }
                }
                intString += numericValue.ToString();
            }
            printString(position, intString, color);
        }

        //Prints a decimal number with a given color on the character grid.
        public void printNumberDouble(Vector2Int position, double numericValue, Color32 color)
        {
            for (int i = 0; i < numericValue.ToString().Length; i++)
            {
                if (numericValue.ToString()[i] == '.')
                {
                    printNumberDouble(position, numericValue, color, (uint)i, (uint)(numericValue.ToString().Length - i) - 1, false);
                    return;
                }
            }
            printNumberDouble(position, numericValue, color, (uint)numericValue.ToString().Length, 0, false);
        }

        //Prints a decimal number with a given color on the character grid.
        public void printNumberDouble(Vector2Int position, double numericValue, Color32 color, uint paddingLength, uint decimalLength, bool addZeros)
        {
            string floatString = "";
            string fullString = numericValue.ToString();
            int fullStringLength = numericValue.ToString().Length;
            int realNumber = (int)numericValue;
            int realNumberLength = realNumber.ToString().Length;
            if (paddingLength - realNumberLength >= 0)
            {
                long counter = paddingLength - realNumberLength;
                for (int i = 0; i < counter; i++)
                {
                    if (addZeros == true)
                    {
                        floatString += "0";
                    }
                    else
                    {
                        floatString += " ";
                    }
                }
                floatString += realNumber.ToString();
                for (int i = realNumberLength; i < decimalLength + realNumberLength + 1; i++)
                {
                    if (i < fullStringLength)
                    {
                        floatString += fullString[i];
                    }
                }
            }
            printString(position, floatString, color);
        }

        //Changes the background color of a character grid space.
        public void printBackground(Vector2Int position, Color32 color)
        {
            _tileIndex[position.x, position.y].GetComponent<GridTile>().BackgroundColor = color;
        }

        //Changes the background color of a rectangle of character grid spaces.
        public void printBackgroundRectangle(Vector2Int position, Color32 color, int width, int height)
        {
            printBackgroundRectangle(position, color, width, height, true);
        }

        //Changes the background color of a rectangle of character grid spaces.
        public void printBackgroundRectangle(Vector2Int position, Color32 color, int width, int height, bool filled)
        {
            if (filled == true)
            {
                for (int yPos = position.y; yPos < position.y + height; yPos++)
                {
                    for (int xPos = position.x; xPos < position.x + width; xPos++)
                    {
                        if (xPos < _gridSize.x && yPos < _gridSize.y)
                        {
                            printBackground(new Vector2Int(xPos, yPos), color);
                        }
                    }
                }
            }
            else
            {
                for (int xPos = position.x; xPos < position.x + width; xPos++)
                {
                    if (xPos < _gridSize.x)
                    {
                        printBackground(new Vector2Int(xPos, position.y), color);
                        if (position.y + (height - 1) < _gridSize.y)
                        {
                            printBackground(new Vector2Int(xPos, position.y + (height - 1)), color);
                        }
                    }
                }
                for (int yPos = position.y; yPos < position.y + height; yPos++)
                {
                    if (yPos < _gridSize.y)
                    {
                        printBackground(new Vector2Int(position.x, yPos), color);
                        if (position.x + (width - 1) < _gridSize.x)
                        {
                            printBackground(new Vector2Int(position.x + width - 1, yPos), color);
                        }
                    }
                }
            }
        }

        //Retrieves the position of the mouse on the character grid. Returns (-1, -1) if outside of the character grid.
        public Vector2Int mousePosition()
        {
            Vector2 mousePos = new Vector2(Mouse.current.position.ReadValue().x, Screen.height - Mouse.current.position.ReadValue().y);
            float anchoredPositionX = _gridImage.GetComponent<RectTransform>().anchoredPosition.x;
            float anchoredPositionY = _gridImage.GetComponent<RectTransform>().anchoredPosition.y;
            float rectWidth = _gridImage.GetComponent<RectTransform>().rect.width;
            float rectHeight = _gridImage.GetComponent<RectTransform>().rect.height;

            if (mousePos.x >= anchoredPositionX &&
                mousePos.y >= anchoredPositionY &&
                mousePos.x < anchoredPositionX + rectWidth &&
                mousePos.y < anchoredPositionY + rectHeight)
            {
                mousePos.x -= anchoredPositionX;
                mousePos.y -= anchoredPositionY;

                mousePos.x = mousePos.x / (rectWidth / _gridSize.x);
                mousePos.y = mousePos.y / (rectHeight / _gridSize.y);

                return new Vector2Int((int)mousePos.x, (int)mousePos.y);
            }

            return new Vector2Int(-1, -1);
        }

        //Sets if character on the character grid is blinking.
        public void characterBlink(Vector2Int position, bool enable)
        {
            _tileIndex[position.x, position.y].GetComponent<GridTile>().BlinkRate = 0.5f;
            _tileIndex[position.x, position.y].GetComponent<GridTile>().BlinkEnable = enable;
        }

        //Sets if character on the character grid is blinking.
        public void characterBlink(Vector2Int position, bool enable, float seconds)
        {
            _tileIndex[position.x, position.y].GetComponent<GridTile>().BlinkRate = seconds;
            _tileIndex[position.x, position.y].GetComponent<GridTile>().BlinkEnable = enable;
        }

        //Sets if character on the character grid is underlined.
        public void characterUnderlined(Vector2Int position, bool enable)
        {
            _tileIndex[position.x, position.y].GetComponent<GridTile>().UnderlineEnable = enable;
        }

        //Sets a character on the character grid to be underlined.
        public void characterUnderlined(Vector2Int position, bool enable, uint width)
        {
            for (int i = 0; i < width; i++)
            {
                if (position.x + i != _gridSize.x)
                {
                    _tileIndex[position.x + i, position.y].GetComponent<GridTile>().UnderlineEnable = enable;
                }
                else { break; }
            }
        }
    }



    //Holds data for each char sprite in a read-only container.
    [System.Serializable]
    public class CharSprite
    {
        public CharSprite(int asciiValueIn, char characterIn, Sprite spriteIn, bool _isUIin, FilterMode filterMode)
        {
            _asciiValue = asciiValueIn;
            _character = characterIn;
            _uSprite = spriteIn;
            _isUI = _isUIin;
            _filterMode = filterMode;
        }

        [SerializeField] private int _asciiValue;
        [SerializeField] private char _character;
        [SerializeField] private Sprite _uSprite;
        [SerializeField] private bool _isUI;
        [SerializeField] private FilterMode _filterMode;

        public int AsciiValue
        {
            get { return _asciiValue; }
        }

        public char Character
        {
            get { return _character; }
        }

        public Sprite Usprite
        {
            get { _uSprite.texture.filterMode = _filterMode; return _uSprite; }
        }

        public bool IsUI
        {
            get { return _isUI; }
        }
    }




    //Holds all information for grid resolutions used for import into UCurses.
    public class DosScreenMode
    {
        private Vector2Int _gridSize;
        private Vector2Int _dosScreenResolution;
        private float _aspectRatio;
        private ScreenAlignment _screenAlignment;
        private float _screenOffset;
        private Vector2Int _characterSize;
        private bool _offsetLine;
        private FilterMode _screenFilterMode;
        private FilterMode _characterFilterMode;


        public DosScreenMode(Vector2Int gridSize, Vector2Int screenResolution, float aspectRatio, ScreenAlignment horizontalAlignment, float horizontalOffset, Vector2Int characterSize, bool offsetLine, FilterMode screenFilterMode, FilterMode characterFilterMode)
        {
            _gridSize = gridSize;
            _dosScreenResolution = screenResolution;
            _aspectRatio = aspectRatio;
            _screenAlignment = horizontalAlignment;
            _screenOffset = horizontalOffset;
            _characterSize = characterSize;
            _offsetLine = offsetLine;
            _screenFilterMode = screenFilterMode;
            _characterFilterMode = characterFilterMode;
        }


        public Vector2Int GridSize
        {
            get { return _gridSize; }
        }

        public Vector2Int DosScreenResolution
        {
            get { return _dosScreenResolution; }
        }

        public float AspectRatio
        {
            get { return _aspectRatio; }
        }

        public ScreenAlignment ScreenAlignment
        {
            get { return _screenAlignment; }
        }

        public float ScreenOffset
        {
            get { return _screenOffset; }
        }

        public Vector2Int CharacterSize
        {
            get { return _characterSize; }
        }

        public bool OffsetLine
        {
            get { return _offsetLine; }
        }

        public FilterMode ScreenFilterMode
        {
            get { return _screenFilterMode; }
        }

        public FilterMode CharacterFilterMode
        {
            get { return _characterFilterMode; }
        }
    }
}