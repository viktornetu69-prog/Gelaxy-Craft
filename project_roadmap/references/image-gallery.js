class ImageGallery {
    constructor(gamesPath) {
        this.gamesPath = gamesPath;
        this.games = new Map();
    }

    async loadGame(gameId) {
        try {
            const response = await fetch(`${this.gamesPath}/${gameId}/images/manifest.json`);
            const manifest = await response.json();
            
            const gameData = {
                id: gameId,
                images: manifest.map(filename => ({
                    name: filename,
                    path: `${this.gamesPath}/${gameId}/images/${filename}`,
                    loaded: false
                }))
            };
            
            this.games.set(gameId, gameData);
            return gameData;
        } catch (error) {
            console.error(`Error loading game ${gameId}:`, error);
            return null;
        }
    }

    getImages(gameId) {
        return this.games.get(gameId);
    }
}

function initGallery(gameId, containerId) {
    const gallery = new ImageGallery('images');
    const container = document.getElementById(containerId);
    
    if (!container) return;
    
    // Create gallery structure
    container.innerHTML = `
        <div class="gallery-header">
            <h3>🖼️ Изображения для ${gameId.replace(/-/g, ' ').toUpperCase()}</h3>
            <div class="gallery-controls">
                <button class="upload-btn" onclick="document.getElementById('fileUpload').click()">
                    📤 Загрузить картинки
                </button>
                <input type="file" id="fileUpload" multiple accept="image/*" style="display:none" onchange="handleFileUpload(event)">
                <span class="image-count">0 изображений загружено</span>
            </div>
        </div>
        <div class="gallery-grid" id="galleryGrid"></div>
        <div class="upload-hint">
            <p>💡 Чтобы добавить картинки:</p>
            <ol>
                <li>Нажмите "Загрузить картинки" и выберите файлы</li>
                <li>Или перетащите файлы в область ниже</li>
                <li>Файлы появятся в папке: references/${gameId}/images/</li>
            </ol>
        </div>
    `;
    
    // Load manifest
    gallery.loadGame(gameId).then(data => {
        if (data) {
            renderGallery(data, container);
            setupDropZone(container);
        }
    });
    
    window.currentGallery = gallery;
    window.currentGameId = gameId;
}

function renderGallery(gameData, container) {
    const grid = container.querySelector('.gallery-grid');
    const count = container.querySelector('.image-count');
    
    if (!grid) return;
    
    grid.innerHTML = '';
    count.textContent = `${gameData.images.filter(img => img.loaded).length}/${gameData.images.length} изображений загружено`;
    
    gameData.images.forEach((img, index) => {
        const imgElement = document.createElement('div');
        imgElement.className = 'gallery-item';
        imgElement.innerHTML = `
            <div class="image-placeholder" onclick="showImage('${img.path}')" style="background: linear-gradient(45deg, #2d2d2d, #1a1a1a);">
                <div class="placeholder-content">
                    <span class="placeholder-icon">🖼️</span>
                    <span class="placeholder-name">${img.name}</span>
                    <span class="placeholder-status">${img.loaded ? '✅ Загружено' : '⏳ Ожидает'}</span>
                </div>
            </div>
        `;
        grid.appendChild(imgElement);
    });
}

function showImage(path) {
    const overlay = document.getElementById('imageOverlay');
    if (overlay) {
        overlay.remove();
    }
    
    const fullOverlay = document.createElement('div');
    fullOverlay.id = 'imageOverlay';
    fullOverlay.className = 'image-overlay';
    fullOverlay.innerHTML = `
        <div class="overlay-content">
            <button class="close-overlay" onclick="this.parentElement.parentElement.remove()">×</button>
            <img src="${path}" alt="Image preview">
        </div>
    `;
    document.body.appendChild(fullOverlay);
}

function setupDropZone(container) {
    const grid = container.querySelector('.gallery-grid');
    if (!grid) return;
    
    grid.addEventListener('dragover', (e) => {
        e.preventDefault();
        grid.style.borderColor = '#58a6ff';
    });
    
    grid.addEventListener('dragleave', () => {
        grid.style.borderColor = '#30363d';
    });
    
    grid.addEventListener('drop', (e) => {
        e.preventDefault();
        grid.style.borderColor = '#30363d';
        
        const files = Array.from(e.dataTransfer.files);
        handleFiles(files);
    });
}

function handleFileUpload(event) {
    const files = Array.from(event.target.files);
    handleFiles(files);
}

function handleFiles(files) {
    const gallery = window.currentGallery;
    const gameId = window.currentGameId;
    
    if (!gallery) return;
    
    files.forEach(file => {
        const reader = new FileReader();
        reader.onload = (e) => {
            const imageData = e.target.result;
            
            // Update manifest
            const gameData = gallery.getImages(gameId);
            if (gameData) {
                const filename = file.name.replace(/\.[^/.]+$/, '') + '.jpg';
                const existingIndex = gameData.images.findIndex(img => img.name === filename);
                
                if (existingIndex >= 0) {
                    gameData.images[existingIndex] = {
                        name: filename,
                        path: `images/${gameId}/images/${filename}`,
                        loaded: true,
                        data: imageData
                    };
                } else {
                    gameData.images.push({
                        name: filename,
                        path: `images/${gameId}/images/${filename}`,
                        loaded: true,
                        data: imageData
                    });
                }
                
                // Re-render
                const container = document.getElementById('galleryContainer');
                if (container) {
                    renderGallery(gameData, container);
                }
            }
        };
        reader.readAsDataURL(file);
    });
}

// CSS styles for gallery
const galleryStyles = `
.gallery-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 20px;
    padding: 15px;
    background: #161b22;
    border-radius: 8px;
    border: 1px solid #30363d;
}

.gallery-controls {
    display: flex;
    align-items: center;
    gap: 15px;
}

.upload-btn {
    background: #238636;
    color: white;
    border: none;
    padding: 10px 20px;
    border-radius: 6px;
    cursor: pointer;
    font-size: 14px;
    transition: all 0.2s;
}

.upload-btn:hover {
    background: #2ea043;
}

.image-count {
    color: #8b949e;
    font-size: 14px;
}

.gallery-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
    gap: 15px;
    padding: 15px;
    background: #0d1117;
    border-radius: 8px;
    border: 2px dashed #30363d;
    min-height: 200px;
}

.gallery-item {
    aspect-ratio: 1;
    border-radius: 8px;
    overflow: hidden;
    cursor: pointer;
    transition: transform 0.2s;
}

.gallery-item:hover {
    transform: scale(1.05);
}

.image-placeholder {
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    border: 1px solid #30363d;
    border-radius: 8px;
}

.placeholder-content {
    text-align: center;
    color: #8b949e;
}

.placeholder-icon {
    font-size: 2em;
    display: block;
    margin-bottom: 8px;
}

.placeholder-name {
    display: block;
    font-size: 12px;
    margin-bottom: 4px;
    color: #c9d1d9;
}

.placeholder-status {
    display: block;
    font-size: 11px;
    color: #3fb950;
}

.upload-hint {
    margin-top: 20px;
    padding: 15px;
    background: rgba(22, 27, 34, 0.8);
    border-radius: 8px;
    border: 1px solid #30363d;
}

.upload-hint h4 {
    color: #58a6ff;
    margin-bottom: 10px;
}

.upload-hint ol {
    color: #8b949e;
    padding-left: 20px;
}

.upload-hint li {
    margin-bottom: 5px;
}

.image-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.9);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
}

.overlay-content {
    position: relative;
    max-width: 90%;
    max-height: 90%;
}

.overlay-content img {
    max-width: 100%;
    max-height: 80vh;
    border-radius: 8px;
}

.close-overlay {
    position: absolute;
    top: -40px;
    right: 0;
    background: none;
    border: none;
    color: white;
    font-size: 2em;
    cursor: pointer;
}
`;

// Inject styles
const styleSheet = document.createElement('style');
styleSheet.textContent = galleryStyles;
document.head.appendChild(styleSheet);
