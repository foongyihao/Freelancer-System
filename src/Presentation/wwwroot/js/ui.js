/**
 * Show an error banner with the provided message (overwrites any existing text).
 * @param {string} message - Error text to display to the user.
 */
export function showError(message){
  const box = document.getElementById('error');
  const span = document.getElementById('errorMsg');
  box.style.display = 'block';
  span.textContent = message;
}

/**
 * Hide and clear the error banner.
 * @returns {void}
 */
export function clearError(){
  const box = document.getElementById('error');
  box.style.display = 'none';
  document.getElementById('errorMsg').textContent='';
}

/**
 * Render freelancer cards into the list container.
 * @param {Array<Object>} items - Freelancer objects.
 * @param {string} items[].id
 * @param {string} items[].username
 * @param {string} items[].email
 * @param {string} [items[].phoneNumber]
 * @param {boolean} items[].isArchived
 * @param {Array<{name:string}>} [items[].skillsets]
 * @param {Array<{name:string}>} [items[].hobbies]
 * @returns {void}
 */
import { handleAction } from './main.js';

export function renderFreelancerCard(items){
  const list = document.getElementById('list');
  list.innerHTML='';
  (items||[]).forEach(f=> {
    const card = document.createElement('div');
    card.className='card'+(f.isArchived?' archived':'');
    card.innerHTML = `
      <strong>${f.username}</strong> <span class="arch" style="display:${f.isArchived?'inline':'none'}">ARCHIVED</span><br>
      <small>${f.email}</small><br>
      <small>${f.phoneNumber||''}</small><br>
      <div class="skills">${(f.skillsets||[]).map(s=>`<span class='badge'>${s.name}</span>`).join('')}</div>
      <div class="hobbies">${(f.hobbies||[]).map(h=>`<span class='badge'>${h.name}</span>`).join('')}</div>
      <div style='margin-top:.5rem;'>
        <button data-act='edit'>Edit</button>
        <button data-act='archive'>${f.isArchived?'Unarchive':'Archive'}</button>
        <button data-act='delete'>Delete</button>
      </div>`;
    card.querySelectorAll('button').forEach(btn=> btn.addEventListener('click', ()=> handleAction(f, btn.dataset.act)));
    list.appendChild(card);
  });
}

/**
 * Update pagination UI state (buttons enable/disable and page info text).
 * @returns {void}
 */
export function updatePager(){
  document.getElementById('pageInfo').textContent = `Page ${currentPage} / ${totalPages}`;
  document.getElementById('pager').style.display = 'flex';
  prevBtn.disabled = currentPage<=1;
  nextBtn.disabled = currentPage>=totalPages;
}

/**
 * Reset the create/edit form and clear hidden id field.
 * @returns {void}
 */
export function resetForm(){
  freelancerForm.reset();
  document.getElementById('freelancerId').value='';
}
