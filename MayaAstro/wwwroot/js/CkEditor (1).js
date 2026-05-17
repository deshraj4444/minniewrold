const customToolbar = [
    '|',
    'heading',
    'paragraph',
    'bold',
    'italic',
    'link',
    'fontSize',
    'fontFamily',
    'fontColor',
    'alignment:left',
    'alignment:center',
    'alignment:right',
    'alignment:justify',
    '|',
    'bulletedList',
    'numberedList',
    '|',
    'imageUpload',
    '|',
    'undo',
    'redo'
];

class MyUploadAdapter {
    constructor(loader) {
        this.loader = loader;
    }

    upload() {
        return this.loader.file
            .then(file => new Promise((resolve, reject) => {
                this._initRequest();
                this._initListeners(resolve, reject, file);
                this._sendRequest(file);
            }));
    }

    abort() {
        if (this.xhr) {
            this.xhr.abort();
        }
    }

    _initRequest() {
        this.xhr = new XMLHttpRequest();
        this.xhr.open('POST', '/CropImage/UploadCKImage', true);
        this.xhr.responseType = 'json';
    }

    _initListeners(resolve, reject, file) {
        const xhr = this.xhr;
        const loader = this.loader;
        const genericErrorText = `Couldn't upload file: ${file.name}.`;

        xhr.addEventListener('error', () => reject(genericErrorText));
        xhr.addEventListener('abort', () => reject());
        xhr.addEventListener('load', () => {
            const response = xhr.response;
            if (!response || response.error) {
                return reject(response && response.error ? response.error.message : genericErrorText);
            }
            resolve({ default: response.url });
        });

        if (xhr.upload) {
            xhr.upload.addEventListener('progress', evt => {
                if (evt.lengthComputable) {
                    loader.uploadTotal = evt.total;
                    loader.uploaded = evt.loaded;
                }
            });
        }
    }

    _sendRequest(file) {
        const data = new FormData();
        data.append('upload', file);
        this.xhr.send(data);
    }
}

function MyCustomUploadAdapterPlugin(editor) {
    editor.plugins.get('FileRepository').createUploadAdapter = (loader) => {
        return new MyUploadAdapter(loader);
    };
}

// 🔹 Select all elements with the class "editor" and initialize CKEditor for each one
document.querySelectorAll('.editor').forEach(editorElement => {
    ClassicEditor.create(editorElement, {
        extraPlugins: [MyCustomUploadAdapterPlugin],  // Removed 'HtmlEmbed'
        toolbar: customToolbar,
        fontSize: {
            options: ['10pt', '12pt', '14pt', '16pt', '18pt', '20pt', '30pt', '40pt'],
        },
        fontFamily: {
            options: [
                'default', 'Arial, sans-serif', 'Georgia, serif', 'Times New Roman, serif',
                'Trebuchet MS, sans-serif', 'Verdana, sans-serif', 'Roboto, sans-serif',
                'Open Sans, sans-serif', 'Lato, sans-serif', 'Montserrat, sans-serif',
                'Raleway, sans-serif', 'Oswald, sans-serif', 'Helvetica, sans-serif',
                'Consolas, monospace', 'Franklin Gothic, sans-serif', 'Calibri, sans-serif'
            ]
        },
        image: {
            toolbar: ['imageTextAlternative', 'imageStyle:full', 'imageStyle:side'],
            upload: { types: ['jpeg', 'png', 'gif', 'bmp', 'webp'] }
        }
    }).catch(error => {
        console.error('CKEditor initialization error:', error);
    });
});
