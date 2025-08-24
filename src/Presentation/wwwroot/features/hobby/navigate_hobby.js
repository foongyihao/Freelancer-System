import { api } from '/features/utils/api.js';

let hobbiesPage = 1;
let hobbiesTotal = 1;
const pageSize = 12;
let hobbiesTerm = '';

function renderHobbies(items){
  const grid = document.getElementById('hobbiesGrid');
  if(!grid) return;
  grid.innerHTML='';
  (items||[]).forEach(h=>{
    const card = document.createElement('div');
    card.className='card';
    card.innerHTML = `
      <strong>${h.name}</strong>
      <div style='margin-top:.5rem;'>
        <button data-act='edit'>Edit</button>
        <button data-act='delete'>Delete</button>
      </div>`;
    card.querySelectorAll('button').forEach(btn=> btn.addEventListener('click', ()=> handleHobbyAction(h, btn.dataset.act)));
    grid.appendChild(card);
  });
  document.getElementById('hobbiesPageInfo').textContent = `Page ${hobbiesPage} / ${hobbiesTotal}`;
  document.getElementById('hobbiesPrev').disabled = hobbiesPage<=1;
  document.getElementById('hobbiesNext').disabled = hobbiesPage>=hobbiesTotal;
}

async function loadHobbies(){
  const q = new URLSearchParams();
  q.set('page', hobbiesPage); q.set('pageSize', pageSize);
  if(hobbiesTerm) q.set('term', hobbiesTerm);
  const data = await api(`/api/v1/hobbies?${q.toString()}`).catch(()=>null);
  hobbiesTotal = data?.totalPages || 1;
  renderHobbies(data?.items||[]);
}

export function handleHobbyAction(h, act){
  if(act==='edit'){
    document.getElementById('hobbyName').value = h.name;
    document.getElementById('hobbyName').dataset.editId = h.id;
  } else if(act==='delete'){
    if(confirm('Delete hobby?')){
      api(`/api/v1/hobbies/${h.id}`, { method:'DELETE' }).then(loadHobbies);
    }
  }
}

function wireHobbyNav(){
  document.getElementById('hobbiesSearch')?.addEventListener('click', ()=>{ hobbiesPage=1; hobbiesTerm = document.getElementById('hobbiesTerm').value.trim(); loadHobbies(); });
  document.getElementById('hobbiesPrev')?.addEventListener('click', ()=>{ if(hobbiesPage>1){ hobbiesPage--; loadHobbies(); }});
  document.getElementById('hobbiesNext')?.addEventListener('click', ()=>{ hobbiesPage++; loadHobbies(); });
}

document.addEventListener('DOMContentLoaded', ()=>{ wireHobbyNav(); loadHobbies(); });
