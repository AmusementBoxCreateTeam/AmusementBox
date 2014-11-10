<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

class Boxes extends CI_Model {

    public function __construct() {
        parent::__construct();
        $this->load->database();
    }
    
    function get_list($search = '', $offset = ''){
        $this->db->select('id');
        $this->db->select('entry_date');
        $this->db->select('X(point) as x');
        $this->db->select('Y(point) as y');
        $this->db->select('delete_date');
        $this->db->from('boxes');
        $this->db->where($this->create_where($search));
        
        $query = $this->db->get();
        
        return $query->result();
    }

    private function create_where($search) {
        $where = array();
        if (!empty($search['pref'])) {
            $where += array('prefectures =' => $search['pref']);
        }
        if (!empty($search['entry_date_over']) {
            $where += array('entry_date >=' => $search['entry_date_over']);
        }
        if (!empty($search['entry_date_under']) {
            $where += array('entry_date <=' => $search['entry_date_under']);
        }

        return $where;
    }

}