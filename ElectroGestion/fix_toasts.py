import os
import re

directory = r'c:\Users\jeiso\Files\Universidad\ISC - Periodos Académicos\7mo - Periodo Acedémico (3-2026)\Proyecto Aplicado\Proyecto\Pagina Web Blazor\ElectroGestion\Components\Pages'

for root, dirs, files in os.walk(directory):
    for file in files:
        if file.endswith('.razor'):
            path = os.path.join(root, file)
            with open(path, 'r', encoding='utf-8') as f:
                content = f.read()
            
            orig_content = content

            # Replace AppState.ShowToast with appropriate ToastService calls
            content = re.sub(r'AppState\.ShowToast\(([^,]+),\s*"warn"\)', r'ToastService.ShowWarning(\1, "Atención")', content)
            content = re.sub(r'AppState\.ShowToast\(([^,]+),\s*"error"\)', r'ToastService.ShowError(\1, "Error")', content)
            content = re.sub(r'AppState\.ShowToast\(([^,]+),\s*"info"\)', r'ToastService.ShowToast(ToastType.Info, "Info", \1)', content)
            content = re.sub(r'AppState\.ShowToast\(([^,]+)\)', r'ToastService.ShowSuccess(\1, "Éxito")', content)

            if orig_content != content:
                # Add @inject ToastService ToastService if not present
                if '@inject ToastService ToastService' not in content:
                    lines = content.split('\n')
                    last_inject_idx = -1
                    for i, line in enumerate(lines):
                        if line.startswith('@inject'):
                            last_inject_idx = i
                    if last_inject_idx != -1:
                        lines.insert(last_inject_idx + 1, '@inject ToastService ToastService')
                    else:
                        lines.insert(0, '@inject ToastService ToastService')
                    content = '\n'.join(lines)
                
                with open(path, 'w', encoding='utf-8') as f:
                    f.write(content)
                print(f'Updated {file}')
