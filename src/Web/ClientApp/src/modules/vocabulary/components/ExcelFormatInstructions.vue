<template>
  <div class="instructions-section">
    <h2>
      <i class="mdi mdi-information"></i>
      Excel File Format Instructions
    </h2>
    <div class="instruction-card">
      <div class="instruction-header">
        <i class="mdi mdi-file-excel text-green"></i>
        <h3>Required Excel Columns</h3>
      </div>
      <div class="columns-grid">
        <div class="column-item">
          <span class="column-name">Word</span>
          <span class="column-desc">The vocabulary word (required)</span>
        </div>
        <div class="column-item">
          <span class="column-name">Definition</span>
          <span class="column-desc">Word meaning/definition (required)</span>
        </div>
        <div class="column-item">
          <span class="column-name">Pronunciation</span>
          <span class="column-desc">IPA pronunciation (optional)</span>
        </div>
        <div class="column-item">
          <span class="column-name">CategoryName</span>
          <span class="column-desc">Category name (optional)</span>
        </div>
        <div class="column-item">
          <span class="column-name">Example_Sentence</span>
          <span class="column-desc">Example sentence (optional)</span>
        </div>
      </div>

      <div class="instruction-tips">
        <h4><i class="mdi mdi-lightbulb"></i> Tips</h4>
        <ul>
          <li>Download the template to see the exact format</li>
          <li>Make sure all required columns are filled</li>
          <li>Use consistent category names for grouping</li>
          <li>One word per row in the Excel file</li>
        </ul>
      </div>
    </div>
  </div>
</template>

<style scoped>
.instructions-section {
  margin: 2rem 0;
}

.instructions-section h2 {
  font-size: 1.5rem;
  margin-bottom: 1rem;
  color: #e9ecef;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.instruction-card {
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.1) 0%, rgba(255, 255, 255, 0.05) 100%);
  border-radius: 12px;
  padding: 2rem;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.instruction-header {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.instruction-header i {
  font-size: 2rem;
}

.text-green {
  color: #51cf66;
}

.instruction-header h3 {
  font-size: 1.25rem;
  color: #e9ecef;
}

.columns-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.column-item {
  background: rgba(255, 255, 255, 0.05);
  padding: 1rem;
  border-radius: 8px;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.column-name {
  font-weight: bold;
  color: #74c0fc;
  font-size: 1rem;
}

.column-desc {
  color: #adb5bd;
  font-size: 0.9rem;
}

.instruction-tips {
  background: rgba(116, 192, 252, 0.1);
  border-left: 4px solid #74c0fc;
  padding: 1rem 1.5rem;
  border-radius: 8px;
}

.instruction-tips h4 {
  color: #74c0fc;
  margin-bottom: 0.75rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.instruction-tips ul {
  margin: 0;
  padding-left: 1.5rem;
  color: #adb5bd;
}

.instruction-tips li {
  margin-bottom: 0.5rem;
}
</style>
