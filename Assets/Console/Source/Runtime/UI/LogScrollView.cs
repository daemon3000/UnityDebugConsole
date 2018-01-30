using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Luminosity.Console.UI
{
	public class LogScrollView : MonoBehaviour
	{
		[SerializeField]
		private GameObject m_entryTemplate;
		[SerializeField]
		private RectTransform m_panel;
		[SerializeField]
		private RectTransform m_container;
		[SerializeField]
		private Slider m_slider;

		private Queue<LogMessageEntry> m_entryPool;
		private LinkedList<LogMessageEntry> m_entries;
		private LogMessageCollection m_messages;
		private int? m_selectedMessageID;
		private int m_requiredNumberOfEntries;
		private int m_lastEntryIndex;
		private float m_entryHeight;

		public event UnityAction<LogMessageEntry> EntrySelected;

		public LogMessageCollection Messages
		{
			get { return m_messages; }
			set { m_messages = value; }
		}

		private float PanelHeight
		{
			get { return m_panel.sizeDelta.y; }
		}

		private void Awake()
		{
			var entryTemplate = m_entryTemplate.GetComponent<LogMessageEntry>();

			m_entryPool = new Queue<LogMessageEntry>();
			m_entries = new LinkedList<LogMessageEntry>();
			m_lastEntryIndex = 0;
			m_entryHeight = entryTemplate.Height;
			m_requiredNumberOfEntries = Mathf.CeilToInt(PanelHeight / m_entryHeight) + 1;

			for(int i = 0; i < m_requiredNumberOfEntries; i++)
			{
				var entry = RetainEntry();
				entry.Hide();
			}

			m_slider.maxValue = 0.0f;
			m_slider.value = 0.0f;
		}

		public void OnShown()
		{
			if(m_messages != null)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
				RefreshView();
				m_messages.ItemAdded += OnMessageAdded;
				m_messages.AllItemsRemoved += OnAllMessagedRemoved;
				m_messages.FilterChanged += OnMessageCollectionChanged;
			}
		}

		public void OnHidden()
		{
			if(m_messages != null)
			{
				m_messages.ItemAdded -= OnMessageAdded;
				m_messages.AllItemsRemoved -= OnAllMessagedRemoved;
				m_messages.FilterChanged -= OnMessageCollectionChanged;
			}
		}

		public void OnViewSizeChanged()
		{
			if(m_messages != null)
			{
				RefreshView();
			}
		}

		private void RefreshView()
		{
			ReleaseAllEntries();

			m_requiredNumberOfEntries = Mathf.CeilToInt(PanelHeight / m_entryHeight) + 1;
			for(int i = 0; i < m_requiredNumberOfEntries; i++)
			{
				var entry = RetainEntry();
				entry.Hide();
			}

			if(m_messages.Count * m_entryHeight > PanelHeight)
			{
				float currentScroll = m_slider.value;

				m_slider.onValueChanged.RemoveListener(OnSliderValueChanged);
				m_slider.maxValue = (m_messages.Count - (PanelHeight / m_entryHeight));
				m_slider.value = currentScroll;
				UpdateEntries(currentScroll, true, false);

				m_slider.onValueChanged.AddListener(OnSliderValueChanged);
			}
			else
			{
				m_slider.onValueChanged.RemoveListener(OnSliderValueChanged);
				m_slider.maxValue = 0.0f;
				m_slider.value = 0.0f;
				UpdateEntries(0.0f, true, false);
			}
		}

		private void OnMessageAdded()
		{
			if(m_messages.Count * m_entryHeight > PanelHeight)
			{
				float currentScroll = m_slider.value;

				m_slider.onValueChanged.RemoveListener(OnSliderValueChanged);
				m_slider.maxValue = (m_messages.Count - (PanelHeight / m_entryHeight));
				m_slider.value = currentScroll;
				UpdateEntries(currentScroll, true, false);

				m_slider.onValueChanged.AddListener(OnSliderValueChanged);
			}
			else
			{
				UpdateEntries(0.0f, true, false);
			}
		}

		private void OnAllMessagedRemoved()
		{
			m_slider.onValueChanged.RemoveListener(OnSliderValueChanged);
			m_slider.value = 0.0f;
			m_slider.maxValue = 0.0f;
			m_lastEntryIndex = 0;

			foreach(var entry in m_entries)
			{
				entry.Hide();
			}
		}

		private void OnMessageCollectionChanged()
		{
			m_selectedMessageID = null;
			if(m_messages.Count * m_entryHeight > PanelHeight)
			{
				m_slider.onValueChanged.RemoveListener(OnSliderValueChanged);
				m_slider.maxValue = (m_messages.Count - (PanelHeight / m_entryHeight));
				m_slider.value = 0.0f;
				UpdateEntries(0.0f, true, true);

				m_slider.onValueChanged.AddListener(OnSliderValueChanged);
			}
			else
			{
				m_slider.onValueChanged.RemoveListener(OnSliderValueChanged);
				m_slider.maxValue = 0.0f;
				m_slider.value = 0.0f;
				UpdateEntries(0.0f, true, true);
			}
		}

		private void OnSliderValueChanged(float value)
		{
			UpdateEntries(value, false, false);
		}

		private void OnLogMessageEntryClicked(LogMessageEntry entry)
		{
			foreach(var item in m_entries)
			{
				if(item.IsSelected)
					item.OnDeselected();
			}

			m_selectedMessageID = entry.MessageID;
			entry.OnSelected();
			if(EntrySelected != null)
			{
				EntrySelected(entry);
			}
		}

		private void UpdateEntries(float scroll, bool forceUpdate, bool fullUpdate)
		{
			int en = (int)scroll;

			if(en >= m_messages.Count && m_messages.Count > 0)
				return;

			if(en != m_lastEntryIndex || forceUpdate)
			{
				LinkedListNode<LogMessageEntry> node;
				int index;

				if(!fullUpdate)
				{
					if(en > m_lastEntryIndex)
					{
						var entry = m_entries.First.Value;
						entry.SetAsLastSibling();

						m_entries.RemoveFirst();
						m_entries.AddLast(entry);
					}
					else if(en < m_lastEntryIndex)
					{
						var entry = m_entries.Last.Value;
						entry.SetAsFirstSibling();

						m_entries.RemoveLast();
						m_entries.AddFirst(entry);
					}
				}

				for(node = m_entries.First, index = 0; node != null; node = node.Next, index++)
				{
					if(en + index < m_messages.Count)
					{
						node.Value.Show(m_messages[en + index], en + index);
						node.Value.UserAlternateBackground = (en + index) % 2 == 1;

						if(m_selectedMessageID == en + index)
						{
							if(!node.Value.IsSelected)
							{
								node.Value.OnSelected();
							}
						}
						else
						{
							if(node.Value.IsSelected)
							{
								node.Value.OnDeselected();
							}
						}
					}
					else
					{
						node.Value.Hide();
					}
				}

				if(fullUpdate)
				{
					Vector2 position = m_container.anchoredPosition;
					position.y = 0.0f;
					m_container.anchoredPosition = position;
				}
				else
				{
					if(en == m_lastEntryIndex)
					{
						Vector2 position = m_container.anchoredPosition;
						position.y = (scroll - en) * m_entryHeight;
						m_container.anchoredPosition = position;
					}
					else if(en > m_lastEntryIndex)
					{
						Vector2 position = m_container.anchoredPosition;
						position.y = 0.0f;
						m_container.anchoredPosition = position;
					}
					else
					{
						Vector2 position = m_container.anchoredPosition;
						position.y = m_entryHeight;
						m_container.anchoredPosition = position;
					}
				}
			}
			else
			{
				Vector2 position = m_container.anchoredPosition;
				position.y = (scroll - en) * m_entryHeight;
				m_container.anchoredPosition = position;
			}

			m_lastEntryIndex = en;
		}

		private LogMessageEntry RetainEntry()
		{
			LogMessageEntry entry = null;
			if(m_entryPool.Count > 0)
			{
				entry = m_entryPool.Dequeue();
			}
			else
			{
				entry = CreateEntry();
			}

			entry.SetAsLastSibling();
			m_entries.AddLast(entry);
			return entry;
		}

		private void ReleaseEntry(LogMessageEntry entry)
		{
			entry.Hide();
			m_entryPool.Enqueue(entry);
		}

		private void ReleaseAllEntries()
		{
			foreach(var entry in m_entries)
			{
				entry.Hide();
				m_entryPool.Enqueue(entry);
			}

			m_entries.Clear();
		}

		private LogMessageEntry CreateEntry()
		{
			GameObject entryGO = Instantiate(m_entryTemplate);
			LogMessageEntry entry = entryGO.GetComponent<LogMessageEntry>();
			entry.SetParent(m_container);
			entry.Clicked += OnLogMessageEntryClicked;

			return entry;
		}
	}
}