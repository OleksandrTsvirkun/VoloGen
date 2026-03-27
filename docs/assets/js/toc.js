(function () {
  var content = document.getElementById('content');
  var tocList = document.getElementById('toc-list');
  var tocSidebar = document.getElementById('toc-sidebar');
  if (!content || !tocList || !tocSidebar) { return; }

  var headings = content.querySelectorAll('h2, h3');
  if (headings.length === 0) {
    tocSidebar.style.display = 'none';
    return;
  }

  for (var i = 0; i < headings.length; i++) {
    var heading = headings[i];
    if (!heading.id) {
      heading.id = heading.textContent.trim().toLowerCase()
        .replace(/[^\w]+/g, '-')
        .replace(/^-+|-+$/g, '');
    }
    var li = document.createElement('li');
    li.className = heading.tagName === 'H3' ? 'toc-h3' : 'toc-h2';
    var a = document.createElement('a');
    a.href = '#' + heading.id;
    a.textContent = heading.textContent;
    li.appendChild(a);
    tocList.appendChild(li);
  }

  /* Highlight active heading on scroll */
  var tocLinks = tocList.querySelectorAll('a');
  window.addEventListener('scroll', function () {
    var current = '';
    for (var j = 0; j < headings.length; j++) {
      if (headings[j].getBoundingClientRect().top <= 80) {
        current = headings[j].id;
      }
    }
    for (var k = 0; k < tocLinks.length; k++) {
      tocLinks[k].classList.toggle('active', tocLinks[k].getAttribute('href') === '#' + current);
    }
  });

  /* Mobile sidebar toggle */
  var toggle = document.getElementById('sidebar-toggle');
  var sidebarContent = document.getElementById('sidebar-content');
  if (toggle && sidebarContent) {
    toggle.addEventListener('click', function () {
      sidebarContent.classList.toggle('open');
    });
  }
})();
