# PreviousLives

A fun desktop application that captures a live webcam snapshot, generates a dramatic past-life description using OpenAI, creates an image-to-image transformation with Stability AI's Stable Diffusion, and stores the results in a local SQLite database.

## Features

* **Webcam Capture**: Snap a photo from your webcam.
* **Past-Life Story**: Generate a richly detailed, epic past-life description via OpenAI's GPT-3.5-turbo.
* **AI Image Transformation**: Use Stability AI's Stable Diffusion (img2img) to artistically reinterpret your photo.
* **Local Storage**: Save the original image, AI-edited image, and description in an SQLite database.
* **Desktop UI**: Windows Forms-based user interface for seamless interaction.

## Prerequisites

* **Python 3.8+**
* **.NET 6.0 SDK** (or newer)
* **Windows OS** (for webcam support & Forms UI)
* **API Keys**:

  * OpenAI API Key
  * Stability AI API Key

## Installation

1. **Clone the repository**:

   ```bash
   git clone https://github.com/Di-Boss/PreviousLives.git
   cd PreviousLives
   ```

2. **Configure Environment Variables**:

   * Windows PowerShell:

     ```powershell
     setx OPENAI_API_KEY "<your_openai_key>"
     setx STABILITY_API_KEY "<your_stability_key>"
     ```
   * Or edit `img2img.py` and hard-code keys (for testing only).

3. **Install Python Dependencies**:

   ```bash
   pip install -r requirements.txt
   ```

   `requirements.txt` should include:

   ```text
   requests
   openai
   ```

4. **Restore .NET Packages**:

   ```bash
   cd PreviousLives.UI
   dotnet restore
   ```

## Database Initialization

The application uses SQLite. On first run, it will automatically create the `captures.db` file under `%LOCALAPPDATA%\PreviousLives` and apply schema migrations. No manual setup is needed.

If you need to inspect or modify the database schema manually, use the [SQLite CLI](https://www.sqlite.org/cli.html) or a GUI tool like [DB Browser for SQLite](https://sqlitebrowser.org/).

## Usage

### 1. Start the Desktop UI

From the `PreviousLives.UI` project folder:

```powershell
cd PreviousLives.UI
dotnet run
```

### 2. Capture a Photo

* Grant webcam permission if prompted.
* The live preview will appear in the main window.
* Click the **CAPTURE** button.

### 3. AI Processing

* The app saves a snapshot (`upload.png`) to its `bin\Debug` folder.
* It calls `img2img.py`:

  * Generates a past-life description via OpenAI.
  * Sends the image to Stability AI for img2img transformation.
  * Saves both images and description to the SQLite database.

### 4. View Results

* Upon success, a second form displays:

  * The original webcam photo.
  * The AI-generated past-life description.
  * (Future: display the edited image.)

## Command-Line Script

`img2img.py` can also be run standalone:

```bash
python img2img.py --image upload.png \
    --profession "Opera Singer" \
    --age 42 \
    --db "%LOCALAPPDATA%\\PreviousLives\\captures.db"
```

### Parameters:

* `--image`: Path to the input PNG image.
* `--profession`: Past-life profession prompt.
* `--age`: Age at death for the narrative.
* `--db`: Path to the SQLite database file.

## Configuration Options

Inside `img2img.py`, you can tweak the Stable Diffusion parameters:

```python
# in call_img2img:
"strength": "0.50",
"sampler":  "k_euler_a",
"steps":    "30",
"cfg_scale": "9.0",
```

Adjust `strength` (0.0–1.0) for the degree of transformation, `steps` for diffusion steps, and `cfg_scale` for prompt adherence.

## Troubleshooting

* **Unicode Errors on Windows**: The script enforces UTF-8 on stdout to avoid encoding issues.
* **Database Locked**: Ensure no other process is holding `captures.db` open.
* **Missing Python or Modules**: Verify `python` is on your PATH and dependencies are installed.
* **Stability AI Credits**: Confirm your API key has sufficient quota; check the raw JSON output for errors.

## License

MIT License © Marin Tonchev

---

Enjoy uncovering your dramatic past lives!
