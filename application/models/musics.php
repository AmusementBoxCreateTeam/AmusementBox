<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

class Musics extends CI_Model {

    public function __construct() {
        parent::__construct();
        $this->load->database();
        $this->load->library('conversion_age_birthday');
    }

    /**
     * 音楽一覧を取得
     * @param type $search
     * @param type $offset
     * @return type
     */
    function get_list($search = '', $offset = '') {

        $this->db->select('songs.id');
        $this->db->select('songs.song_title');
        $this->db->select('songs.lyricist');
        $this->db->select('songs.composer');
        $this->db->select('songs.singer');
        $this->db->select('songs.song_time');
        $this->db->select('songs.genre');
        $this->db->select('songs.release_date');
        $this->db->from('songs');
        //検索条件の生成
        $where = $this->create_where($search);
        $this->db->where($where);
        $this->config->load('user');
        $this->db->limit($this->config->item('disp_num'), $offset);
        $query = $this->db->get();

        return $query->result();
    }

    /**
     * 音楽一覧の件数を取得
     * @return type
     */
    function get_count_row($search) {
        $this->db->select('songs.id');
        $this->db->select('songs.song_title');
        $this->db->select('songs.lyricist');
        $this->db->select('songs.composer');
        $this->db->select('songs.singer');
        $this->db->select('songs.song_time');
        $this->db->select('songs.genre');
        $this->db->select('songs.release_date');
        $this->db->from('songs');
        //検索条件の生成
        $where = $this->create_where($search);
        $this->db->where($where);

        $query = $this->db->get();

        return $query->num_rows();
    }

    function get_use_count($id) {
        $this->db->select('count(user_id) as use_count');
        $this->db->from('historys');
        $this->db->where('user_id', $id);
        $this->db->group_by('user_id');

        $query = $this->db->get();

        if ($query->num_rows() > 0) {
            return $query->row()->use_count;
        } else {
            return 0;
        }
    }

    function create_where($search = '') {
        $where = array();
        if (!empty($search['gender']) && empty($search['gender'][1])) {
            $where += array('gender' => $search['gender'][0]);
        }
        if (!empty($search['age_over'])) {
            $date = $this->conversion_age_birthday->conversion_date($search['age_over']);
            $where += array('birthday <=' => $date);
        }
        if (!empty($search['age_under'])) {
            $date = $this->conversion_age_birthday->conversion_date($search['age_under']);
            $where += array('birthday >=' => $date);
        }
        if (!empty($search['entry_date_over'])) {
            $where += array('entry_date >=' => $search['entry_date_over']);
        }
        if (!empty($search['entry_date_under'])) {
            $where += array('entry_date <=' => $search['entry_date_under']);
        }

        return $where;
    }

    function get_user_detail($user_id) {
        $this->db->select('users.id');
        $this->db->select('users.entry_date');
        $this->db->select('users.update_date');
        $this->db->select('users.nickname');
        $this->db->select('users.birthday');
        $this->db->select('users.gender');
        $this->db->from('users');
        $this->db->where('users.id', $user_id);
        $query = $this->db->get();

        if ($query->num_rows() < 0) {
            show_404();
            exit;
        }

        return $query->result();
    }

    function get_history_detail($user_id) {
        $this->db->distinct();
        $this->db->select('historys.use_datetime');
        $this->db->select('boxes.id as box_id');
        $this->db->select('songs.id as song_id');
        $this->db->select('songs.song_title');
        $this->db->select('songs.lyricist');
        $this->db->select('songs.composer');
        $this->db->select('songs.singer');
        $this->db->from('historys');
        $this->db->where('songs.id', $user_id);
        $this->db->join('songs', 'historys.song_id = songs.id', 'left');
        $this->db->join('boxes', 'historys.box_id = boxes.id', 'left');
        $this->db->join('users', 'historys.user_id = users.id', 'left');

        $query = $this->db->get();

        if ($query->num_rows() < 0) {
            show_404();
            exit;
        }

        return $query->result();
    }

}
